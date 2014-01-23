using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace QueueIT.Security.Tests
{
    [TestClass]
    public class DefaultKnownUserUrlProviderTest
    {

        [TestMethod]
        [ExpectedException(typeof(ApplicationException))]
        public void DefaultKnownUserUrlProvider_Constructor_RequestArgumentNull_Test()
        {
            //Arrange

            //Act
            new DefaultKnownUserUrlProvider();

            //Assert        
        }

        [TestMethod]
        public void DefaultKnownUserUrlProvider_GetUrl_Test()
        {
            //Arrange
            Uri expectedUrl = new Uri("http://some.url/somepath/default.aspx?x=sdfs");
            HttpRequest request = new HttpRequest("default.aspx", expectedUrl.AbsoluteUri, "x=sdfs");
            HttpContext.Current = new HttpContext(request, new HttpResponse(null));

            DefaultKnownUserUrlProvider provider = new DefaultKnownUserUrlProvider();

            //Act
            Uri actualUrl = provider.GetUrl();

            //Assert      
            Assert.AreEqual(expectedUrl, actualUrl);
        }

        [TestMethod]
        public void DefaultKnownUserUrlProvider_GetUrl_MissingSlash_Test()
        {
            //Arrange
            Uri expectedUrl = new Uri("http://some.url/");
            HttpRequest request = new HttpRequest("default.aspx", "http://some.url", null);
            HttpContext.Current = new HttpContext(request, new HttpResponse(null));

            DefaultKnownUserUrlProvider provider = new DefaultKnownUserUrlProvider();

            //Act
            Uri actualUrl = provider.GetUrl();

            //Assert      
            Assert.AreEqual(expectedUrl, actualUrl);
        }

    }
}
