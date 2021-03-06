using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;
using ALinq;
using ALinq.Mapping;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NorthwindDemo;

namespace Test
{

    [TestClass]

    public class OdpOracleTest : NorOracleTest
    {
        public new static Func<OdpOracleNorthwind, string, IQueryable<Customer>> CustomersByCity =
           ALinq.CompiledQuery.Compile((OdpOracleNorthwind db, string city) =>
                                                        from c in db.Customers
                                                        where c.City == city
                                                        select c);
        public new static Func<OdpOracleNorthwind, string, Customer> CustomersById =
            ALinq.CompiledQuery.Compile((OdpOracleNorthwind db, string id) =>
                                               Enumerable.Where(db.Customers, c => c.CustomerID == id).First());

        public override NorthwindDatabase CreateDataBaseInstace()
        {
            var xmlMapping = XmlMappingSource.FromStream(GetType().Assembly.GetManifestResourceStream("Test.Northwind.Oracle.Odp.map")); //XmlMappingSource.FromUrl("Northwind.Oracle.Odp.map");
            writer = Console.Out;
            return new OdpOracleNorthwind("Data Source=vpc1;Persist Security Info=True;User ID=Northwind;Password=Test;", xmlMapping) { Log = writer };
        }

        private static bool copy;
        [ClassInitialize]
        public new static void Initialize(TestContext testContext)
        {
            //if (!copy)
            //{
            //    var type = typeof(SQLiteTest);
            //    var path = type.Module.FullyQualifiedName;
            //    var filePath = Path.GetDirectoryName(path) + @"\ALinq.Oracle.Odp.lic";
            //    File.Copy(@"E:\ALinqs\ALinq1.8\Test\ALinq.Oracle.Odp.lic", filePath);
            //    filePath = Path.GetDirectoryName(path) + @"\Northwind.Oracle.Odp.map";
            //    File.Copy(@"E:\ALinqs\ALinq1.8\Test\Northwind.Oracle.Odp.map", filePath);

            //    copy = true;
            //}
            writer = new StreamWriter("c:/Oracle.txt", false);
            //var database = new OracleNorthwind(OracleNorthwind.CreateConnection("Northwind", "Northwind", "localhost")) { Log = writer };
            //if (!database.DatabaseExists())
            //{
            //    database.CreateDatabase();
            //    database.Connection.Close();
            //}
        }

        [TestMethod]
        public override void CreateDatabase()
        {
            //writer = new StreamWriter("c:/Oracle.txt", false);
            var constr = string.Format(string.Format("Data Source=vpc1;User ID={0};password={1}", "System", "test"));
            var xmlMapping = XmlMappingSource.FromStream(GetType().Assembly.GetManifestResourceStream("Test.Northwind.Oracle.Odp.map"));
            var database = new OdpOracleNorthwind(constr, xmlMapping) { Log = Console.Out };//new OdpOracleNorthwind(constr) { Log = Console.Out };//
            if (database.DatabaseExists())
                database.DeleteDatabase();
            try
            {
                database.CreateDatabase();
            }
            catch (Exception)
            {
                database.Log.Flush();
                database.Log.Close();
                throw;
            }
            finally
            {
                database.Dispose();
            }
        }

        [TestMethod]
        public override void StoreAndReuseQuery()
        {
            var customers = CustomersByCity((OdpOracleNorthwind)db, "London").ToList();
            Assert.IsTrue(customers.Count() > 0);

            var id = customers.First().CustomerID;
            var customer = CustomersById((OdpOracleNorthwind)db, id);
            Assert.AreEqual("London", customer.City);
        }

        [TestMethod]
        public override void StringConnect()
        {
            var connstr = string.Format("Data Source={0};User ID=Northwind;Password=Test", NorthwindDatabase.DB_HOST);
            var context = new ALinq.DataContext(connstr, typeof(ALinq.Oracle.Odp.OracleProvider));
            context.Connection.Open();
            context.Connection.Close();
        }

        //[TestMethod]
        //public new void Procedure_AddCategory()
        //{
        //    db.Log = Console.Out;
        //    var categoryID = db.Categories.Max(o => o.CategoryID) + 1;
        //    ((OdpOracleNorthwind)db).AddCategory(categoryID, "category", "description");
        //}

        //存储过程
        //1、标量返回
        //[TestMethod]
        //public new void Procedure_GetCustomersCountByRegion()
        //{
        //    var groups = db.Customers.GroupBy(o => o.Region).Select(g => new { Count = g.Count(), Region = g.Key }).ToArray();
        //    var regions = db.Regions.ToArray();
        //    foreach (var group in groups)
        //    {
        //        if (group.Region == null)
        //            continue;
        //        var count1 = group.Count;
        //        var count2 = ((OdpOracleNorthwind)db).GetCustomersCountByRegion(group.Region);
        //        Assert.AreEqual(count1, count2);
        //    }
        //}

        //2、单一结果集返回
        //[TestMethod]
        //public new void Procedure_GetCustomersByCity()
        //{
        //    var groups = db.Customers.GroupBy(o => o.City).Select(g => new { g.Key, Count = g.Count() }).ToArray();
        //    foreach (var group in groups)
        //    {
        //        if (group.Key == null)
        //            continue;
        //        object myrc;
        //        var result = ((OdpOracleNorthwind)db).GetCustomersByCity(group.Key, out myrc).ToList();
        //        Assert.AreEqual(group.Count, result.Count());
        //    }
        //}

        //3.多个可能形状的单一结果集
        //[TestMethod]
        //public new void Procedure_SingleRowset_MultiShape()
        //{
        //    //返回全部Customer结果集
        //    var result = ((OdpOracleNorthwind)db).SingleRowset_MultiShape(1, null);
        //    var shape1 = result.GetResult<Customer>();
        //    foreach (var compName in shape1)
        //    {
        //        Console.WriteLine(compName.CompanyName);
        //    }

        //    //返回部分Customer结果集
        //    result = ((OdpOracleNorthwind)db).SingleRowset_MultiShape(2, null);
        //    var shape2 = result.GetResult<PartialCustomersSetResult>();
        //    foreach (var con in shape2)
        //    {
        //        Console.WriteLine(con.ContactName);
        //    }
        //}

        //4.多个结果集
        //[TestMethod]
        //public new void Procedure_GetCustomerAndOrders()
        //{
        //    var result = ((OdpOracleNorthwind)db).GetCustomerAndOrders("SEVES", null, null);
        //    //返回Customer结果集
        //    var customers = result.GetResult<Customer>();

        //    //返回Orders结果集
        //    var orders = result.GetResult<Order>();

        //    //在这里，我们读取CustomerResultSet中的数据
        //    foreach (var cust in customers)
        //    {
        //        Console.WriteLine(cust.CustomerID);
        //    }

        //    foreach (var order in orders)
        //    {
        //        Console.WriteLine(order.OrderID);
        //    }
        //}

        [TestMethod]
        public void Functions()
        {
            //int? result;
            //result = ((OdpOracleNorthwind)db).Function1();
            //Assert.AreEqual(100, result);
            //result = ((OdpOracleNorthwind)db).Function2(10);
            //Assert.AreEqual(10, result);

            //result = ((OdpOracleNorthwind)db).Function2(-1);
            //Assert.AreEqual(null, result);

            //result = ((OdpOracleNorthwind)db).Dual.Select(o => Function1()).SingleOrDefault();
            //Assert.AreEqual(100, result);
            //result = ((OdpOracleNorthwind)db).Dual.Select(o => Function2(10)).SingleOrDefault();
            //Assert.AreEqual(10, result);

            //string str;
            //str = ((OdpOracleNorthwind)db).FUN_STRING(-1);
            //Assert.AreEqual(null, str);
            //str = ((OdpOracleNorthwind)db).Dual.Select(o => FUN_STRING()).SingleOrDefault();
            //Assert.AreEqual("HELLO", str);

            //str = ((OdpOracleNorthwind)db).Dual.Select(o => FUN_STRING(-1)).SingleOrDefault();
            //Assert.AreEqual(null, str);

            //str = ((OdpOracleNorthwind)db).Dual.Select(o => FUN_STRING(1)).SingleOrDefault();
            //Assert.AreEqual("ONE", str);

            //((OdpOracleNorthwind)db).Products.Select(o => new { ProductID = Function2(o.ProductID), o.ProductName }).ToArray();
        }

        [Function]
        public static int Function1()
        {
            throw new NotSupportedException();
        }

        [Function]
        public static int Function2(
            [Parameter(Name = ":pama1")]
            int value)
        {
            throw new NotSupportedException();
        }

        [Function]
        public static string FUN_STRING()
        {
            throw new NotSupportedException();
        }

        [Function(Name = "FUN_STRING1")]
        public static string FUN_STRING(int num)
        {
            throw new NotSupportedException();
        }

        [Function(Name = "SYSDATE")]
        public static DateTime Now()
        {
            throw new NotSupportedException();
        }

        [TestMethod]
        public new void CRUD_Insert2()
        {
            var db = base.db as OdpOracleNorthwind;

            db.Log = Console.Out;
            db.DataTypes.Insert(o => new DataType { Guid = Guid.NewGuid(), Enum = NorthwindDatabase.Enum.Item1, DateTime = db.Now() });

            var id = db.DataTypes.Max(o => o.ID) + 1;
            db.DataTypes.Insert(o => new DataType { ID = id, Guid = Guid.NewGuid(), Enum = NorthwindDatabase.Enum.Item1 });
        }


    }
}