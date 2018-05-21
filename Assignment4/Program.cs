using System;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Transactions;

namespace Assignment4
{
    class Section
    {
        public int courseDateId { get; set;  }
        public int courseId { get; set; }
        public string startDay { get; set; }
        public int length { get; set; }
        public int maxStudentCount { get; set; }
    }

    class Schedule
    {
        public static bool AbortTx()
        {
            Console.Write("Abort the Transaction(y/n)?");
            return Console.ReadLine() == "y";
        }

        public static void DisplayTransactionInformation(string title, TransactionInformation ti)
        {
            if (ti != null)
            {
                Console.WriteLine(title);
                Console.WriteLine("Creation Time: {0:T}", ti.CreationTime);
                Console.WriteLine("Status: {0}", ti.Status); Console.WriteLine("Local ID: {0}", ti.LocalIdentifier);
                Console.WriteLine("Distributed ID: {0}", ti.DistributedIdentifier);
                Console.WriteLine();
            }
        }

        public void addToCourseSchedule(Section aSection)


        {
            string source = "server=(local);" +
                      "integrated security=SSPI;" +
                      "database=CourseManagement";
            SqlConnection connection = new SqlConnection(source);
            SqlCommand cmd = connection.CreateCommand();
            cmd.CommandText = "INSERT INTO CourseDates (CourseDateId, " +
                    "CourseId, StartDay, Length, MaxStudentCount) " +
                    "VALUES (@CourseDateId, @CourseId, @StartDay, @Length, " +
                    "@MaxStudentCount)";
            connection.Open();
            SqlTransaction tx = connection.BeginTransaction();

            try
            {
                cmd.Transaction = tx;

                cmd.Parameters.AddWithValue("@CourseDateId", aSection.courseDateId);
                cmd.Parameters.AddWithValue("@CourseId", aSection.courseId);
                cmd.Parameters.AddWithValue("@StartDay", aSection.startDay);
                cmd.Parameters.AddWithValue("@Length", aSection.length);
                cmd.Parameters.AddWithValue("@MaxStudentCount", aSection.maxStudentCount);

                cmd.ExecuteNonQuery();

                tx.Commit();
            }
            catch (Exception ex)
            {
                Trace.WriteLine("Error: " + ex.Message);
                tx.Rollback();
            }
            finally
            {
                connection.Close();
            }
        }

        public void addToCourseSchedule(Section aSection, Transaction tx)
        {
            string source = "server=(local);" +
                      "integrated security=SSPI;" +
                      "database=CourseManagement";
            SqlConnection connection = new SqlConnection(source);
            connection.Open();

            try
            {
                if (tx != null)
                    connection.EnlistTransaction(tx);

                SqlCommand cmd = connection.CreateCommand();

                cmd.CommandText = "INSERT INTO CourseDates (" +
                    "CourseId, StartDay, Length, MaxStudentCount) " +
                    "VALUES (@CourseId, @StartDay, @Length, " +
                    "@MaxStudentCount)";
                
                cmd.Parameters.AddWithValue("@CourseId", aSection.courseId);
                cmd.Parameters.AddWithValue("@StartDay", aSection.startDay);
                cmd.Parameters.AddWithValue("@Length", aSection.length);
                cmd.Parameters.AddWithValue("@MaxStudentCount", aSection.maxStudentCount);

                cmd.ExecuteNonQuery();
            }
            finally
            {
                connection.Close();
            }
        }

        public static void CommittableTransaction()
        {
            var tx = new CommittableTransaction();
            DisplayTransactionInformation("TX created",
                  tx.TransactionInformation);

            try
            {
                var s1 = new Section
                {
                    courseId = 114,
                    startDay = "01/01/2019",
                    length = 100,
                    maxStudentCount = 60                
                };

                var db = new Schedule();
                db.addToCourseSchedule(s1, tx);

                if (AbortTx())
                {
                    throw new ApplicationException("transaction abort");
                }

                tx.Commit();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine();
                tx.Rollback();
            }

            DisplayTransactionInformation("TX completed",
                  tx.TransactionInformation);
        }
    }
}


class Program
{
        static void Main(string[] args)
    {
        Assignment4.Schedule aSchedule = new Assignment4.Schedule();
        Assignment4.Schedule.CommittableTransaction();
    }
}
