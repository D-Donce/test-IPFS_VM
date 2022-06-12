using System;
using System.IO;
using System.Globalization;
using System.Text;
using System.Security.Cryptography;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace IPFS_VM
{
    class Program
    {
        static void Main(string[] args)
        {
            while(true)
            {
                DateTime utcDate = DateTime.UtcNow;
                var culture = new CultureInfo("en-US");
                string[] a = utcDate.ToString(culture).Split(' '),
                    b = a[1].Split(':');

                if (Int16.Parse(b[2]) % 5 == 0)
                {
                    Console.WriteLine(utcDate.ToString(culture));
                    IPFS program = new IPFS();
                    program.chaining();
                    Thread.Sleep(1000);
                }
            }
        }
    }

    class IPFS
    {

        public string Block { get; set; }
        public void chaining()
        {
            string path = @"C:\ipfs_directory";
            string all_transaction = Soket(); //<<계획 변경 string빌더로 묶어 받아와야함
            string blockhash;
            string time;

            if (Directory.Exists(path) == false)
            {
                Directory.CreateDirectory(path);
            } //디렉토리 판별 'ipfs_directory'
            if (Directory.Exists(path + @"\formain") == false)
            {
                Directory.CreateDirectory(path + @"\formain");
            } //디렉토리 판별 'formain'


            DateTime utcDate = DateTime.UtcNow;
            var culture = new CultureInfo("en-US");
            time = utcDate.ToString(culture);
            blockhash = SHA256Hash(time); //블럭 헤더
            Console.WriteLine("{0} - UTC date and time: {1}\n{2}", culture.NativeName, utcDate.ToString(culture), blockhash);

            string pathblock = path + @"\formain\" + blockhash;
            if (Directory.Exists(pathblock) == false)
            {
                Directory.CreateDirectory(pathblock);//블럭 생성

                Directory.CreateDirectory(pathblock + @"\Body");//블럭 바디
                string all_transaction_head = Transaction(pathblock + @"\Body", all_transaction);
                if (!File.Exists(path))//블럭헤더
                {
                    File.Create(pathblock + @"\Header.txt");
                    TxtWrite(pathblock + @"\Header.txt", "이전 블록 해시" + "//" + MerkleTree(all_transaction_head) + "//" + time);
                }
            } //블럭 판별 생성
        }

        public string Transaction(string path, string data)
        {
            //트렌젝션

            //data 파싱

            string pasing = null;
            string all_transaction_head = null; //스트링 빌더 사용

            TxtWrite(path, pasing);
            return all_transaction_head;
        }
        public string MerkleTree(string path)
        {
            //헤싱을 통한 트리구조 ','로 구분하여 정렬
            //스트링 빌더 사용
            return null;
        }
        private void TxtWrite(string path, string abcd)
        {
            StreamWriter writer;
            writer = File.AppendText(path);
            writer.WriteLine(abcd);
            writer.Close();
        }

        private string SHA256Hash(string data)
        {
            SHA256 sha = new SHA256Managed();
            byte[] hash = sha.ComputeHash(Encoding.ASCII.GetBytes(data));
            StringBuilder stringBuilder = new StringBuilder();
            foreach (byte b in hash)
            {
                stringBuilder.AppendFormat("{0:x2}", b);
            }
            return stringBuilder.ToString();
        }

        public string Soket()
        {
            string data = null;
            var ipep = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 10000);
            // 소켓 인스턴스 생성
            using (Socket client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
            {
                // 소켓 접속
                client.Connect(ipep);
                // 접속이 되면 Task로 병렬 처리
                new Task(() =>
                {
                    try
                    {
                        // 종료되면 자동 client 종료
                        // 통신 바이너리 버퍼
                        var binary = new Byte[1024];
                        // 서버로부터 메시지 대기 
                        client.Receive(binary);
                        // 서버로 받은 메시지를 String으로 변환
                        data = Encoding.ASCII.GetString(binary).Trim('\0');
                        // 메시지 내용을 콘솔에 표시
                        Console.Write(data);
                    }
                    catch (SocketException)
                    {
                        // 접속 끝김이 발생하면 Exception이 발생
                    }
                    // Task 실행
                }).Start();
                return data;
            }
        }
    }
}
