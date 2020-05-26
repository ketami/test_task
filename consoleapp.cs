using System;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using System.Threading;
using Npgsql.PostgresTypes;
using System.Data.SqlTypes;
using System.Data;

namespace ConsoleApp2
{
    public class Param
    {
        
        public string arg0;
        public string arg1;
        public int arg2;
        public Param(string _arg0, string _arg1, int _arg2){
            this.arg0 = _arg0;
            this.arg1 = _arg1;
            this.arg2 = _arg2;

        }
       public static void Registration(NpgsqlConnection con, string username)
        {
            var checkuser = "INSERT INTO customers(username) VALUES (@username) ON CONFLICT ON CONSTRAINT username_unique DO NOTHING;";
            using var cmd = new NpgsqlCommand(checkuser, con);
            {
                NpgsqlParameter prm = new NpgsqlParameter();
                prm.ParameterName = "@username";
                prm.Value = username;
                cmd.Parameters.Add(prm);
            }
            cmd.ExecuteNonQuery();
            {
             // Console.WriteLine("ACCOUNT, " + username);
            }
        }
        static void Buying(NpgsqlConnection con, string arg0, string arg1, int arg2)
        {
            var checkreserve = "select * from ((reserve join products using (prodid)) join customers using (custid)) where username = @username and prodname = @prodname;";
            using var cmd = new NpgsqlCommand(checkreserve, con);
            {
                NpgsqlParameter prm = new NpgsqlParameter();
                prm.ParameterName = "@username";
                prm.Value = arg0;
                cmd.Parameters.Add(prm);
                NpgsqlParameter prm2 = new NpgsqlParameter();
                prm2.ParameterName = "@prodname";
                prm2.Value = arg1;
                cmd.Parameters.Add(prm2);
                NpgsqlParameter prm3 = new NpgsqlParameter();
                prm3.ParameterName = "@amount";
                prm3.Value = arg2;
                cmd.Parameters.Add(prm3);
            }
            var startTime2 = System.Diagnostics.Stopwatch.StartNew();
            try
            {
                Console.WriteLine("Начинает покупку " + arg0 + " " + arg2 + " " + arg1);


                var startTime = System.Diagnostics.Stopwatch.StartNew();
                cmd.CommandText = "begin work; update products set remain = remain - @amount where prodname = @prodname;insert into reserve(custid, prodid, amount)values ((select custid from customers where username = @username),(select prodid from products where prodname = @prodname),@amount)ON CONFLICT ON CONSTRAINT reserve_pk DO update set amount=reserve.amount+@amount where (reserve.custid = ((select custid from customers where username=@username)) and (reserve.prodid= (select prodid from products where prodname=@prodname)));commit work;";
                cmd.ExecuteNonQuery();

                startTime.Stop();
                startTime2.Stop();
                var resultTime = startTime.Elapsed;
                // elapsedTime - строка, которая будет содержать значение затраченного времени
                string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:000}",
                    resultTime.Hours,
                    resultTime.Minutes,
                    resultTime.Seconds,
                    resultTime.Milliseconds);
                Console.WriteLine("Заканчивает покупку " + arg0 + " за " + elapsedTime);
            }
            catch (Npgsql.PostgresException ex)
            {
                var resultTime2 = startTime2.Elapsed;
                // elapsedTime - строка, которая будет содержать значение затраченного времени
                string elapsedTime2 = String.Format("{0:00}:{1:00}:{2:00}.{3:000}",
                    resultTime2.Hours,
                    resultTime2.Minutes,
                    resultTime2.Seconds,
                    resultTime2.Milliseconds);
                if (ex.SqlState == "23514")
                {
                    Console.WriteLine("Извините, " + arg0 + ", товар закончился." + " Выполнено за " + elapsedTime2);
                    Program.CHECKREMAIN = false;
                }
                else
                {
                    Console.WriteLine("Неизвестная ошибка.");
                }
            }
        }

        public void Starts()
        {
            var cs = "Host=localhost;Username=postgres;Password=1234;Database=n_melkov";

            using var con = new NpgsqlConnection(cs);
            con.Open();
            Registration(con, arg0); 
            Buying(con, arg0, arg1, arg2);
        }
    }
    class Program
    {
        public static bool CHECKREMAIN = true;
        static void Main(string[] args)
        {
            Param param = new Param("first","ked", 1);
            Param param2 = new Param("second", "ked", 2);
            Param param3 = new Param("third", "hat", 1);
            Param param4 = new Param("pula", "hat", 2);
            Param param5 = new Param("svaga", "shirt", 1);
            Param param6 = new Param("tupa", "shirt", 2);           

            while (CHECKREMAIN == true)
            {
            Thread myThread = new Thread(new ThreadStart(param.Starts));
            Thread myThread2 = new Thread(new ThreadStart(param2.Starts));
            Thread myThread3 = new Thread(new ThreadStart(param3.Starts));
            Thread myThread4 = new Thread(new ThreadStart(param4.Starts));
            Thread myThread5 = new Thread(new ThreadStart(param5.Starts));
            Thread myThread6 = new Thread(new ThreadStart(param6.Starts));
                myThread.Start();
                myThread2.Start();
                myThread3.Start();
                myThread4.Start();
                myThread5.Start();
                myThread6.Start();
                Thread.Sleep(50);
            }
        }

    }
}
