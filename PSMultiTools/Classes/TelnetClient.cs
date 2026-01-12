using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace PSMultiTools.Classes
{

    public class TelnetClient
    {

        public TcpClient TCPSocket;
        public NetworkStream NewNetworkStream;

        public TelnetClient(string HostName, int Port)
        {
            TCPSocket = new TcpClient();
            TCPSocket.Connect(HostName, Port);
            NewNetworkStream = TCPSocket.GetStream();
        }

        public void Write(string Command)
        {
            if (NewNetworkStream is null)
                return;

            string CommandWithNewLine = Command + "\r\n";
            byte[] CommandBytes = Encoding.ASCII.GetBytes(CommandWithNewLine);
            NewNetworkStream.Write(CommandBytes, 0, CommandBytes.Length);
            NewNetworkStream.Flush();
        }

        public string Read()
        {
            if (NewNetworkStream is null)
                return string.Empty;

            var ResponseBuilder = new StringBuilder();
            var Buffer = new byte[1025];

            while (NewNetworkStream.DataAvailable)
            {
                int BytesRead = NewNetworkStream.Read(Buffer, 0, Buffer.Length);
                if (BytesRead <= 0)
                    break;
                ResponseBuilder.Append(Encoding.ASCII.GetString(Buffer, 0, BytesRead));
                Thread.Sleep(50);
            }

            return ResponseBuilder.ToString();
        }

        public void Close()
        {
            NewNetworkStream?.Close();
            TCPSocket?.Close();
        }

    }
}