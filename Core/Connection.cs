using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Net.Security;
using System.Security.Authentication;
using System.Text.RegularExpressions;
using System.Text;
using Microsoft.VisualBasic;
using Microsoft.VisualBasic.CompilerServices;

namespace GenieClient.Genie
{
    public class Connection
    {

        public event EventConnectedEventHandler EventConnected;

        public delegate void EventConnectedEventHandler();

        public event EventDisconnectedEventHandler EventDisconnected;

        public delegate void EventDisconnectedEventHandler();

        public event EventDataSentEventHandler EventDataSent;

        public delegate void EventDataSentEventHandler();

        public event EventDataRecieveEndEventHandler EventDataRecieveEnd;

        public delegate void EventDataRecieveEndEventHandler();

        public event EventParseRowEventHandler EventParseRow;

        public delegate void EventParseRowEventHandler(StringBuilder row);

        public event EventParsePartialRowEventHandler EventParsePartialRow;

        public delegate void EventParsePartialRowEventHandler(string row);

        public event EventPrintTextEventHandler EventPrintText;

        public delegate void EventPrintTextEventHandler(string text);

        public event EventPrintErrorEventHandler EventPrintError;

        public delegate void EventPrintErrorEventHandler(string text);

        public event EventConnectionLostEventHandler EventConnectionLost;

        public delegate void EventConnectionLostEventHandler();

        public enum SocketErrorCodes
        {
            InterruptedFunctionCall = 10004,
            PermissionDenied = 10013,
            BadAddress = 10014,
            InvalidArgument = 10022,
            TooManyOpenFiles = 10024,
            ResourceTemporarilyUnavailable = 10035,
            OperationNowInProgress = 10036,
            OperationAlreadyInProgress = 10037,
            SocketOperationOnNonSocket = 10038,
            DestinationAddressRequired = 10039,
            MessgeTooLong = 10040,
            WrongProtocolType = 10041,
            BadProtocolOption = 10042,
            ProtocolNotSupported = 10043,
            SocketTypeNotSupported = 10044,
            OperationNotSupported = 10045,
            ProtocolFamilyNotSupported = 10046,
            AddressFamilyNotSupported = 10047,
            AddressInUse = 10048,
            AddressNotAvailable = 10049,
            NetworkIsDown = 10050,
            NetworkIsUnreachable = 10051,
            NetworkReset = 10052,
            ConnectionAborted = 10053,
            ConnectionResetByPeer = 10054,
            NoBufferSpaceAvailable = 10055,
            AlreadyConnected = 10056,
            NotConnected = 10057,
            CannotSendAfterShutdown = 10058,
            ConnectionTimedOut = 10060,
            ConnectionRefused = 10061,
            HostIsDown = 10064,
            HostUnreachable = 10065,
            TooManyProcesses = 10067,
            NetworkSubsystemIsUnavailable = 10091,
            UnsupportedVersion = 10092,
            NotInitialized = 10093,
            ShutdownInProgress = 10101,
            ClassTypeNotFound = 10109,
            HostNotFound = 11001,
            HostNotFoundTryAgain = 11002,
            NonRecoverableError = 11003,
            NoDataOfRequestedType = 11004
        }

        private TcpClient _client;
        private const int MAX_PACKET_SIZE = 2048;
        private SslStream sslStream;

        private Socket m_SocketClient;
        private readonly object m_oSendLock = new object();
        private IPEndPoint m_IPEndPoint;
        private StringBuilder m_ParseBuffer = new StringBuilder();
        private StringBuilder m_RowBuffer = new StringBuilder();

        // The parse buffers are touched from two different thread-pool callbacks -- the receive
        // loop and the disconnect callback -- with no synchronisation. During the key-server to
        // game-server handoff both are live at once: the old socket's disconnect callback flushes
        // a trailing newline through the very buffers the new connection is already filling,
        // which can split or interleave the first lines of the session.
        private readonly object m_oParseLock = new object();

        // Bumped every time a connection is established. The disconnect callback carries the
        // generation it belonged to, so a socket that is already superseded stays out of the
        // current connection's buffers.
        private int m_iConnectionGeneration = 0;

        private DateTime m_oLastServerActivity = DateTime.Now;

        public DateTime LastServerActivity
        {
            get
            {
                return m_oLastServerActivity;
            }
        }

        public bool IsConnected
        {
            get
            {
                if (Information.IsNothing(m_SocketClient))
                {
                    return false;
                }
                else
                {
                    if(_client != null) return _client.Connected;
                    return m_SocketClient.Connected;
                }
            }
        }

        private string m_sHostname = string.Empty;

        public void Connect(string sHostname, int iPort)
        {
            try
            {
                lock (m_oParseLock)
                {
                    m_RowBuffer.Clear(); // Reset row buffer
                    m_ParseBuffer.Clear(); // Reset parse buffer
                    m_iConnectionGeneration += 1;
                }
                if (!Information.IsNothing(m_SocketClient))
                {
                    if (m_SocketClient.Connected == true)
                    {
                        m_SocketClient.Disconnect(false);
                    }

                    m_SocketClient = null;
                }

                m_sHostname = sHostname;
                _client = new TcpClient();
                m_SocketClient = _client.Client;
                _client.Connect(sHostname, iPort);
                m_oLastServerActivity = DateTime.Now;
                PrintText(Utility.GetTimeStamp() + " Connected to " + m_sHostname + ".");
                Recieve(_client);
                EventConnected?.Invoke();
            }
            catch (SocketException ex)
            {
                PrintSocketError("Connect failed", ex.ErrorCode);
                EventConnectionLost?.Invoke();
            }
        }

        public void ConnectAndAuthenticate(string sHostname, int iPort)
        {
            try
            {
                lock (m_oParseLock)
                {
                    m_RowBuffer.Clear(); // Reset row buffer
                    m_ParseBuffer.Clear(); // Reset parse buffer
                    m_iConnectionGeneration += 1;
                }
                if (!Information.IsNothing(m_SocketClient))
                {
                    if (m_SocketClient.Connected == true)
                    {
                        m_SocketClient.Disconnect(false);
                    }

                    m_SocketClient = null;
                }

                m_sHostname = sHostname;
                _client = new TcpClient();
                m_SocketClient = _client.Client;
                
                _client.Connect(sHostname, iPort);
                m_oLastServerActivity = DateTime.Now;
                try
                {
                    sslStream = new SslStream(_client.GetStream(), true, new RemoteCertificateValidationCallback(Utility.ValidateServerCertificate), null);
                    sslStream.ReadTimeout = 500; // .5s — prevents auth/char-select reads from hanging forever
                    try
                    {
                        sslStream.AuthenticateAsClient(m_sHostname, null, SslProtocols.Tls12, false);
                    }
                    catch (AuthenticationException e)
                    {
                        // Must not fall through: everything below assumes a working TLS stream.
                        // Announcing "Connected" and raising EventConnected after a failed
                        // handshake drove the login straight into a disposed client, where the
                        // resulting exception was swallowed by the thread pool and the player
                        // was left staring at a window that never did anything.
                        PrintError("Unable to Authenticate: " + e.Message);
                        sslStream.Dispose();
                        sslStream = null;
                        _client.Dispose();
                        EventConnectionLost?.Invoke();
                        return;
                    }
                    // Complete the connection

                    PrintText(Utility.GetTimeStamp() + " Connected to " + m_sHostname + ".");

                    EventConnected?.Invoke();
                }
                catch (SocketException ex)
                {
                    PrintSocketError("Connect failed", ex.ErrorCode);
                    EventConnectionLost?.Invoke();
                }
            }
            catch (SocketException ex)
            {
                PrintSocketError("Connect failed", ex.ErrorCode);
                EventConnectionLost?.Invoke();
            }
        }
        public enum AuthState
        {
            Disconnected,
            Unauthenticated,
            ListeningForKey,
            KeyAuthenticated,
            Authenticated,
            AuthenticationFailed,
            InvalidResponse
        }
        private AuthState CurrentAuthState = AuthState.Unauthenticated;
        // Every read and write below can throw -- the stream has a 500ms read timeout, and the
        // caller runs this from a Task, so anything that escapes becomes an unobserved task
        // exception: the connect dies in complete silence, with no message and no reconnect.
        // Turn that into a state the caller can report instead.
        public AuthState Authenticate(string account, string password)
        {
            try
            {
                return AuthenticateInternal(account, password);
            }
            catch (Exception ex)
            {
                PrintError(Utility.GetTimeStamp() + " Login failed: " + ex.Message);
                if (sslStream != null)
                {
                    try { sslStream.Dispose(); } catch { }
                    sslStream = null;
                }

                CurrentAuthState = AuthState.Disconnected;
                return CurrentAuthState;
            }
        }

        private AuthState AuthenticateInternal(string account, string password)
        {
            if (_client == null || !_client.Connected || sslStream == null)
            {
                CurrentAuthState = AuthState.Disconnected;
                return CurrentAuthState; //not connected
            }
            else if (account == null || password == null)
            {
                CurrentAuthState = AuthState.Unauthenticated;
                return CurrentAuthState; //No credentials provided.
            }
            else
            {
                // Send K - Key Request
                byte[] message = Encoding.Default.GetBytes("K" + Environment.NewLine);
                sslStream.Write(message);
                sslStream.Flush();

                CurrentAuthState = AuthState.ListeningForKey;
                // Read Key response: should be 32 bytes
                byte[] buffer = new byte[MAX_PACKET_SIZE];
                int bytes = sslStream.Read(buffer, 0, buffer.Length);
                if (bytes != 32)
                {
                    sslStream.Dispose();
                    sslStream = null;
                    CurrentAuthState = AuthState.InvalidResponse;
                    return CurrentAuthState;
                }

                // SslStreams require a byte array to write
                // BlockCopy is used to allow concacatantion, and avoid encoding issues from cyte array -> string -> byte array.
                message = new byte[account.Length + password.Length + 3];
                Buffer.BlockCopy(Encoding.Default.GetBytes("A\t" + account.ToUpper() + "\t"), 0, message, 0, account.Length + 3);
                Buffer.BlockCopy(Utility.EncryptText(buffer, password), 0, message, account.Length + 3, password.Length);
                sslStream.Write(message);
                sslStream.Flush();

                // null out password to not keep it in memory longer than necessary
                password = null;

                buffer = new byte[MAX_PACKET_SIZE];
                _ = sslStream.Read(buffer, 0, buffer.Length);

                if (Encoding.Default.GetString(buffer).Contains("\tKEY\t"))
                {
                    CurrentAuthState = AuthState.KeyAuthenticated;
                }
                else
                {
                    sslStream.Dispose();
                    sslStream = null;
                    CurrentAuthState = AuthState.AuthenticationFailed;
                }
            }
            return CurrentAuthState;
            

            
        }

        // SGE protocol responses are newline-terminated; SslStream.Read may return partial data,
        // so loop until we have a complete response. The G response also sends a trailing blank
        // SGE G response: a single raw chunk with no newline terminator — just read once.
        private string ReadSgeGameInfoResponse()
        {
            byte[] buffer = new byte[MAX_PACKET_SIZE];
            try
            {
                int bytes = sslStream.Read(buffer, 0, buffer.Length);
                if (bytes == 0) return string.Empty;
                return Encoding.Default.GetString(buffer, 0, bytes).TrimEnd('\0', '\r', '\n');
            }
            catch (System.IO.IOException) { /* read timeout or connection closed — same as ReadSgeResponse */ }
            return string.Empty;
        }

        // SGE C/L responses: newline-terminated, may span multiple SSL records for large character lists.
        // Skips any leading blank lines left over from the G response terminator.
        private string ReadSgeResponse()
        {
            byte[] buffer = new byte[MAX_PACKET_SIZE];
            var sb = new StringBuilder();
            try
            {
                while (true)
                {
                    int bytes = sslStream.Read(buffer, 0, buffer.Length);
                    if (bytes == 0) break;
                    string chunk = Encoding.Default.GetString(buffer, 0, bytes);
                    sb.Append(chunk);
                    if (chunk.IndexOfAny(new[] { '\n', '\r' }) >= 0) break;
                }
            }
            catch (System.IO.IOException) { /* read timeout or connection closed — return what we have */ }
            return sb.ToString().TrimEnd('\0', '\r', '\n');
        }

        // Same reasoning as Authenticate: this runs on a Task, so an escaping IOException from
        // the 500ms read timeout would kill the login silently. Report it through the same
        // "E<tab>message" channel the caller already knows how to print.
        public string GetLoginKey(string instance, string character)
        {
            try
            {
                return GetLoginKeyInternal(instance, character);
            }
            catch (Exception ex)
            {
                if (sslStream != null)
                {
                    try { sslStream.Dispose(); } catch { }
                    sslStream = null;
                }

                CurrentAuthState = AuthState.Disconnected;
                return "E\tLogin failed: " + ex.Message;
            }
        }

        private string GetLoginKeyInternal(string instance, string character)
        {
                        // Sanity checks
            if (!IsConnected || sslStream == null)
            {
                return "E\tThe connection was lost.";
            }

            if (string.IsNullOrWhiteSpace(instance))
            {
                return "E\tThe game instance was not specified.";
            }

            if (CurrentAuthState == AuthState.AuthenticationFailed)
            {
                return "E\tAuthentication Failed.";
            }

            // Send G - Game Details Request
            byte[] message = Encoding.Default.GetBytes("G\t" + instance.ToUpper());
            sslStream.Write(message);
            sslStream.Flush();

            //Validate Access - list of status responses:
            // Known good status:
            //  "FREE_TO_PLAY" "PAYING" "PREMIUM" "NORMAL"
            // Known bad status:
            //  "NEW_TO_GAME" "EXPIRED"
            // Unknown status:
            //  "BETA" "FREE" "INTERNAL" "NEED_BILL" "NO_ACCESS" "SHAREWARE" "TRIAL" "UNKNOWN"
            //Check for  match of known good status, and if no match, no access to requested instance
            string gResponse = ReadSgeGameInfoResponse();
            if (gResponse.ToUpper() == "PROBLEM")
            {
                sslStream.Dispose();
                sslStream = null;
                CurrentAuthState = AuthState.Disconnected;
                return "E\tThere is a problem with your account. Please log in to the play.net website for more information.";
            }

            // send C - Character Slot Request
            message = Encoding.Default.GetBytes("C");
            sslStream.Write(message);
            sslStream.Flush();

            string characterResponse = ReadSgeResponse().ToUpper();
            // Requesting character list with no character name given
            if (string.IsNullOrWhiteSpace(character))
            {
                sslStream.Dispose();
                sslStream = null;
                CurrentAuthState = AuthState.Disconnected;
                return characterResponse;
            }

            // Looking for specific character to get login key for
            List<string> characterKeys = characterResponse.Split('\t').ToList<string>();
            string characterKey = string.Empty;
            string lastKey = string.Empty;
            foreach(string key in characterKeys)
            {
                if (key.ToUpper().Equals(character.ToUpper()))
                {
                    characterKey = lastKey;
                    break;
                }
                else
                {
                    lastKey = key;
                }
            }

            if (string.IsNullOrWhiteSpace(characterKey))
            {
                sslStream.Dispose();
                sslStream = null;
                CurrentAuthState = AuthState.Disconnected;
                return "E\tThe specified character was not found: " + character + ".";
            }

            //send L - Login Key Request
            message = Encoding.Default.GetBytes("L\t" + characterKey + "\tSTORM");
            sslStream.Write(message);
            sslStream.Flush();

            CurrentAuthState = AuthState.Authenticated;
            string loginKey = ReadSgeResponse();
            sslStream.Dispose();
            sslStream = null;
            return loginKey;
        }

        public void Disconnect(bool ExitOnDisconnect = false)
        {
            Disconnect(m_SocketClient, ExitOnDisconnect);
        }

        public void Send(string sText)
        {
            Send(m_SocketClient, sText);
        }

        public void Send(byte[] bytes)
        {
            Send(m_SocketClient, bytes);
        }

        private void Disconnect(Socket ConnectedSocket, bool ExitOnDisconnect = false)
        {
            if (Information.IsNothing(ConnectedSocket))
            {
                return;
            }

            if (ConnectedSocket.Connected == true)
            {
                // PrintText("Disconnecting from: " & s.RemoteEndPoint.ToString())

                int iGeneration;
                lock (m_oParseLock)
                {
                    iGeneration = m_iConnectionGeneration;
                }

                ConnectedSocket.BeginDisconnect(false, new AsyncCallback(DisconnectCallback), new object[] { ConnectedSocket, ExitOnDisconnect, iGeneration });
            }

            m_SocketClient = null;
        }

        private void DisconnectCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the socket from the state object
                Socket s = (Socket)(ar.AsyncState as object[])[0];
                bool ExitOnDisconnect = (bool)(ar.AsyncState as object[])[1];
                int iGeneration = (int)(ar.AsyncState as object[])[2];
                // Complete the connection
                s.EndDisconnect(ar);

                // Only flush the buffers if they still belong to this socket. During the
                // key-server to game-server handoff a newer connection has already taken them
                // over, and pushing a trailing newline through at that point splits the first
                // lines the new connection has started to receive.
                bool bBuffersAreStillOurs;
                lock (m_oParseLock)
                {
                    bBuffersAreStillOurs = (iGeneration == m_iConnectionGeneration);
                }

                if (bBuffersAreStillOurs)
                {
                    ParseData(System.Environment.NewLine); // Show lines not yet sent out
                }
                PrintText(Utility.GetTimeStamp() + " Connection closed.");
                if (ExitOnDisconnect)
                {
                    System.Windows.Forms.Application.Exit();
                }
                else
                {
                    EventDisconnected?.Invoke();
                }
                
            }
            catch (SocketException ex)
            {
                PrintSocketError("Connection lost", ex.ErrorCode);
            }
        }

        private void Send(Socket s, string sText)
        {
            try
            {
                if (Information.IsNothing(s) == true)
                {
                    return;
                }

                if (s.Connected == false)
                {
                    return;
                }

                var ByteData = Encoding.Default.GetBytes(sText);
                lock (m_oSendLock)
                {
                    s.BeginSend(ByteData, 0, ByteData.Length, SocketFlags.None, new AsyncCallback(SendCallback), s);
                }
            }
            catch (SocketException ex)
            {
                PrintSocketError("Connection failure", ex.ErrorCode);
            }
        }

        private void Send(Socket s, byte[] ByteData)
        {
            try
            {
                if (Information.IsNothing(s) == true)
                {
                    return;
                }

                if (s.Connected == false)
                {
                    return;
                }

                lock (m_oSendLock)
                {
                    s.BeginSend(ByteData, 0, ByteData.Length, SocketFlags.None, new AsyncCallback(SendCallback), s);
                }
            }
            catch (SocketException ex)
            {
                PrintSocketError("Connection failure", ex.ErrorCode);
            }
        }
        
        private void SendCallback(IAsyncResult ar)
        {
            try
            {
                Socket s = (Socket)ar.AsyncState;
                int bytes = s.EndSend(ar);
                if (bytes > 0)
                {
                    EventDataSent?.Invoke();
                }
            }
            catch (SocketException ex)
            {
                PrintSocketError("Connection failure", ex.ErrorCode);
            }
        }

        private class StateObject
        {
            // Client socket
            public TcpClient oSocketRef;
            // Size of recieve Buffer
            public const int iBufferSize = 10240;
            // Recieve Buffer
            public byte[] oBuffer = new byte[10241];
        }

        private void Recieve(TcpClient s)
        {
            try
            {
                var oState = new StateObject();
                oState.oSocketRef = s;
                s.Client.BeginReceive(oState.oBuffer, 0, StateObject.iBufferSize, SocketFlags.None, new AsyncCallback(ReceiveCallback), oState);
            }
            catch (SocketException ex)
            {
                PrintSocketError("Connection lost", ex.ErrorCode);
                EventConnectionLost?.Invoke();
            }
        }

        private void ReceiveCallback(IAsyncResult ar)
        {
            m_oLastServerActivity = DateTime.Now;
            try
            {
                StateObject oState = (StateObject)ar.AsyncState;
                TcpClient s = oState.oSocketRef;
                if (s.Connected == true)
                {
                    int bytes = s.Client.EndReceive(ar);
                    if (bytes > 0)
                    {
                        if(CurrentAuthState == AuthState.ListeningForKey || CurrentAuthState == AuthState.KeyAuthenticated)
                        {
                            s.Client.BeginReceive(oState.oBuffer, 0, StateObject.iBufferSize, SocketFlags.None, new AsyncCallback(ReceiveCallback), oState);
                            return;
                        }
                        // Append data
                        ParseData(Encoding.Default.GetString(oState.oBuffer, 0, bytes));
                        // Event to update Output
                        DataRecieveEnd();

                        // Get the rest of the data.
                        s.Client.BeginReceive(oState.oBuffer, 0, StateObject.iBufferSize, SocketFlags.None, new AsyncCallback(ReceiveCallback), oState);
                    }
                    else
                    {
                        // Disconnect
                        Disconnect();
                        EventConnectionLost?.Invoke();
                    }
                }
            }
            catch (SocketException ex)
            {
                PrintSocketError("Connection lost", ex.ErrorCode);
                EventConnectionLost?.Invoke();
            }
        }

        private void ParseData(string sText)
        {
            // Serialised so a receive callback and a disconnect callback cannot interleave
            // halfway through a row. Monitor is re-entrant, so a handler that ends up back in
            // here on this thread (a trigger that disconnects, say) still works.
            lock (m_oParseLock)
            {
                char lastchar = 'x';
                foreach (char c in sText)
                {
                    if (c == '\r' || (c == '\n' && lastchar != '\r'))
                    {
                        m_RowBuffer.Append(m_ParseBuffer);
                        m_RowBuffer.Append(System.Environment.NewLine);
                        ParseRow(m_RowBuffer); // Event for parse row
                        m_RowBuffer.Clear();
                        m_ParseBuffer.Clear();
                    }
                    else if (c != '\n' & c != '\a')
                    {
                        m_ParseBuffer.Append(c);
                    }
                    lastchar = c;
                }

                // Broken Line (Print and save result to RowBuffer)
                if (m_ParseBuffer.Length > 0)
                {
                    var buffer = m_ParseBuffer.ToString();
                    ParsePartialRow(buffer);	// Event for partial parse row
                    m_RowBuffer.Append(m_ParseBuffer);
                    m_ParseBuffer.Clear();
                }
            }
        }

        private void ParseRow(StringBuilder oText)
        {
            // For Trigger Events
            EventParseRow?.Invoke(oText);
        }

        private void ParsePartialRow(string sText)
        {
            // For Key Server & Handshake
            EventParsePartialRow?.Invoke(sText);
        }

        private void PrintText(string sText)
        {
            EventPrintText?.Invoke(sText + System.Environment.NewLine);
            EventDataRecieveEnd?.Invoke();
        }

        private void PrintError(string sText)
        {
            EventPrintError?.Invoke(sText + System.Environment.NewLine);
            EventDataRecieveEnd?.Invoke();
        }

        private void DataRecieveEnd()
        {
            EventDataRecieveEnd?.Invoke();
        }

        private void PrintSocketError(string text, int errorcode)
        {
            SocketErrorCodes sec = (SocketErrorCodes)errorcode;
            PrintError(Conversions.ToString(Utility.GetTimeStamp() + " " + text + ". (" + Interaction.IIf(Information.IsNothing(sec), "Unknown", sec.ToString()) + ")"));
        }
    }
}