using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OfficeOpenXml;
using System.IO;
using System.Reflection;

namespace EPPlusTest
{
    [TestClass]
    public abstract class TestBase
    {
        protected ExcelPackage _pck;
        protected static string _clipartPath="";
        protected static string _worksheetPath= @"c:\epplusTest\Testoutput\";
        protected static string _testInputPath = @"c:\epplusTest\workbooks\";
        public TestContext TestContext { get; set; }

        [TestInitialize]
        public void InitBase()
        {
            _pck = new ExcelPackage();
        }

        protected ExcelPackage OpenPackage(string name, bool delete=false)
        {
            var fi = new FileInfo(_worksheetPath + name);
            if(delete && fi.Exists)
            {
                fi.Delete();
            }
            _pck = new ExcelPackage(fi);
            return _pck;
        }
        protected ExcelPackage OpenTemplatePackage(string name)
        {
            var t = new FileInfo(_testInputPath + name);
            if (t.Exists)
            {
                var fi = new FileInfo(_worksheetPath + name);
                _pck = new ExcelPackage(fi, t);
            }
            else
            {
                Assert.Inconclusive($"Template {name} does not exist in path {_testInputPath}");
            }
            return _pck;
        }

        protected void SaveWorksheet(string name)
        {
            if (_pck.Workbook.Worksheets.Count == 0) return;
            var fi = new FileInfo(_worksheetPath + name);
            if (fi.Exists)
            {
                fi.Delete();
            }
            _pck.SaveAs(fi);
        }

        [AssemblyInitialize]
        public static void AssemblyInitialize(TestContext tc)
        {
            _clipartPath = Path.Combine(Path.GetTempPath(), @"EPPlus clipart");
            if (!Directory.Exists(_clipartPath))
            {
                Directory.CreateDirectory(_clipartPath);
            }
            if (Environment.GetEnvironmentVariable("EPPlusTestInputPath") != null)
            {
                _testInputPath = Environment.GetEnvironmentVariable("EPPlusTestInputPath");
            }
            var asm = Assembly.GetExecutingAssembly();
            var validExtensions = new[]
                {
                    ".gif", ".wmf"
                };

            foreach (var name in asm.GetManifestResourceNames())
            {
                foreach (var ext in validExtensions)
                {
                    if (name.EndsWith(ext, StringComparison.OrdinalIgnoreCase))
                    {
                        string fileName = name.Replace("EPPlusTest.Resources.", "");
                        using (var stream = asm.GetManifestResourceStream(name))
                        using (var file = File.Create(Path.Combine(_clipartPath, fileName)))
                        {
                            stream.CopyTo(file);
                        }
                        break;
                    }
                }
            }

            //_worksheetPath = Path.Combine(Path.GetTempPath(), @"EPPlus worksheets");
            if (!Directory.Exists(_worksheetPath))
            {
                _worksheetPath = Path.Combine(Path.GetTempPath(), @"EPPlus_worksheets");
                Directory.CreateDirectory(_worksheetPath);
            }
            var di = new DirectoryInfo(_worksheetPath);
            _worksheetPath = di.FullName + "\\";

            Console.WriteLine("Switching to en-us locale");
            System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");
            // System.Threading.Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo("en-US");
        }


        public static byte[] GetResource(string name)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = "EPPlusTest.Resources." + name;
            using (var stream = assembly.GetManifestResourceStream(resourceName))
            {
                if (stream == null)
                {
                    throw new Exception($"Resource {resourceName} not found in {assembly.FullName}.  Valid resources are: {String.Join(", ", assembly.GetManifestResourceNames())}.");
                }
                using (var ms = new MemoryStream())
                {
                    stream.CopyTo(ms);
                    return ms.ToArray();
                }
            }
        }

        public static System.Drawing.Bitmap GetBitmap(string name)
        {
            return new System.Drawing.Bitmap(typeof(TestBase), $"Resources.{name}");
        }
    }
}
