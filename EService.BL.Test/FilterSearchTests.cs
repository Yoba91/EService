using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using EService.Data.Entity;
using NUnit.Framework;

namespace EService.BL.Test
{
    [TestFixture]
    class FilterSearchTests
    {
        private ParameterExpression parameter;
        private FilterSearch filterSearch;
        
        [SetUp]
        public void InicializeTest()
        {            
            parameter = Expression.Parameter(typeof(ServiceLog), "s");
            filterSearch = new FilterSearch(parameter);

        }

        [Test]
        public void SetWhat_123_123StringReturn()
        {
            // arrange
            string search = "123";
            string expected = "123";

            // act
            filterSearch.SetWhat(search);
            var result = filterSearch.SearchString;

            // assert
            Assert.AreEqual(expected, result);

        }

        [Test]
        public void SetWhere_DeviceAndRowid_ExpressionDeviceRowidReturn()
        {
            // arrange
            string device = "Device";
            string rowid = "Rowid";
            string expected = "s.Device.Rowid";

            // act
            filterSearch.SetWhere(device, rowid);
            Expression result = filterSearch.Member;
            // assert
            Assert.AreEqual(expected, result.ToString());
        }

        [Test]
        public void AddWhere_Member_Requairer()
        {
            // arrange
            MemberExpression member = null;

            // act
            filterSearch.AddWhere(member);

            // assert
            Assert.IsNotEmpty(filterSearch.Members);
        }
    }
}
