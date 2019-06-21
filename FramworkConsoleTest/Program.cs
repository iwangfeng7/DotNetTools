using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FramworkTools;
namespace FramworkConsoleTest
{
    class Program
    {
        static void Main(string[] args)
        {
            MapToTest();
        }

        public static void MapToTest()
        {
            var student11 = new Student1() { Id = 1, Name = "wf", Pic = "wfpic" };
            var student33 = new Student3() { Id = 1, Name = "wf" };

            #region 方案一
            var student2 = Mapping.MapTo<Student2>(student11);
            var student3 = Mapping.MapTo<Student3>(student11);
            var student4 = Mapping.MapTo<Student1>(student33);
            #endregion

            #region 方案二
            var student5 = Mapping.JsonMapTo<Student2>(student11);
            var student6 = Mapping.JsonMapTo<Student2>(student33);
            #endregion

            #region 方案三
            var student7 = Mapping.AutoMapperTo<Student2,Student1>(student11);
            var student8 = Mapping.AutoMapperTo<Student2,Student3>(student33);
            #endregion

            #region 方案四
            var student9 = ExpressionGenericMapper.Map<Student1, Student2>(student11);
            var student10 = ExpressionGenericMapper.Map<Student3, Student2>(student33);
            #endregion

        }
    }

    public class Student1
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Pic { get; set; }
    }
    public class Student2
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Pic { get; set; }
    }
    public class Student3
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
}
