﻿namespace ORM
{

    class Student : Model<Student>
    {
        public String name;
        public Int32 age;
        public String? surname;
    }

    class Program
    {
        static void Main(string[] args)
        {
            var student = new Student();

            var students = student.Select(s => new { s.age, s.name, s.surname });
            // .Execute();

            Console.WriteLine("//////////");

            student.Select(s => s.age)
                    .Where(s => s.age == 22);
            // .Execute();
        }
    }
}