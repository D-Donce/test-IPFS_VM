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
    public static class Global
    {
        public static string beforehash = null;
        //난의도 5 고정
        public static int difficulty_level = 5;
    }
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

                if (Int16.Parse(b[2]) % Global.difficulty_level == 0)
                {
                    Console.WriteLine("미국 국제 표준시 : " + utcDate.ToString(culture));
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
            //파일 경로 설정
            string path = @"C:\ipfs_directory";

            //소켓 데이터 수신
            string all_transaction = Soket();

            //블록 해시, 타임 string변수 선언
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

            string pathblock = path + @"\formain\" + blockhash;
            if (Directory.Exists(pathblock) == false)
            {
                Directory.CreateDirectory(pathblock);//블럭 생성

                Directory.CreateDirectory(pathblock + @"\Body");//블럭 바디

                if (!File.Exists(path))//블럭 헤더
                {
                    Random randomObj = new Random();
                    int max = randomObj.Next(20);

                    TxtWrite(pathblock + @"\Header.txt",
                        "//HeaderJustBefore:"+ Global.beforehash +
                        "//MerkleTree:" + MerkleTree(Transaction(pathblock + @"\Body", all_transaction, max)) +
                        "//Timestamp:" + time +
                        "//difficulty:" + Global.difficulty_level.ToString());

                    //헤더 생성 성공
                    Console.WriteLine("{0} - UTC date and time: {1}\n{2}", culture.NativeName, utcDate.ToString(culture), blockhash);
                    Global.beforehash = blockhash;
                }
            } //블럭 판별 생성
        }

        public string Transaction(string path1, string data, int max)
        {
            StringBuilder all_transaction_header = new StringBuilder();

            for (int i = -1; i < max; i++)
            {
                DateTime utcDate = DateTime.UtcNow;
                var culture = new CultureInfo("en-US");

                string time = utcDate.ToString(culture);
                string transaction_header = SHA256Hash(time + i.ToString());
                string path2 = path1 + @"\" + transaction_header + ".txt";

                StringBuilder transaction_body = new StringBuilder();
                transaction_body.Append("Layertype:formain//Timestamp:");
                transaction_body.Append(time);
                transaction_body.Append("//");
                transaction_body.Append(i.ToString());
                transaction_body.Append("//ref_hash:null&hash_type:null//ref_hash:null&hash_type:null//");
                transaction_body.Append("DKey");
                transaction_body.Append("//");

                //바이너리 데이터를 string으로 변환시 실행오류 발생
                transaction_body.Append(data);

                TxtWrite(path2, transaction_body.ToString());

                all_transaction_header.Append(transaction_header + "//");
            }

            return all_transaction_header.ToString();
        }
        public string MerkleTree(string all_transaction_header)
        {
            string[] transaction_headsers = all_transaction_header.Split("//");
            int length = transaction_headsers.Length;

            StringBuilder merkledata = new StringBuilder();

            while (length != 0)
            {
                string[] arr = new string[length];

                for (int i = 0; i < length-2; i+=3)
                {
                    merkledata.Append(SHA256Hash(transaction_headsers[i] + transaction_headsers[i + 1]));
                    merkledata.Append("//");
                }
                length /= 2;
                merkledata.Append("||");
            }


            return merkledata.ToString();
        }
        private void TxtWrite(string path, string abcd)
        {
            StreamWriter writer;
            writer = File.CreateText(path);
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
