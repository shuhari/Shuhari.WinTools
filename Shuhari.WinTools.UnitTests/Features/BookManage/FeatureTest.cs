using System.IO;
using System.Linq;
using NUnit.Framework;
using BookManageFeature = Shuhari.WinTools.Core.Features.BookManage.Feature;

namespace Shuhari.WinTools.UnitTests.Features.BookManage
{
    [TestFixture]
    public class FeatureTest
    {
        private BookManageFeature _feature;

        [SetUp]
        public void SetUp()
        {
            _feature = new BookManageFeature();
        }

        [TestCase("Apress.Beginning.Backdrop.CMS.pdf", "Apress.Beginning.Backdrop.CMS.pdf")]
        [TestCase("Apress.Beginning.Backdrop.CMS.1484219694.pdf", "Apress.Beginning.Backdrop.CMS.pdf")]
        [TestCase("Apress.Beginning.Backdrop.CMS.331924275X.pdf", "Apress.Beginning.Backdrop.CMS.pdf")]
        public void TrimBookFileName(string oldName, string newName)
        {
            Assert.AreEqual(newName, _feature.TrimFileName(oldName));
        }

        [TestCase("Apress.Beginning.Backdrop.CMS.pdf")]
        [TestCase("Apress.Beginning.Backdrop.CMS.epub")]
        [TestCase("Apress.Beginning.Backdrop.CMS.mobi")]
        public void GetFilesToDelete_OneFile(string fileName)
        {
            Assert.AreEqual(0, GetFilesToDelete(fileName).Length);
        }

        [Test]
        public void GetFilesToDelete_Pdf_Epub_CannotDelete()
        {
            Assert.AreEqual(0, GetFilesToDelete("Apress.Beginning.Backdrop.CMS.pdf", "Apress.Beginning.Backdrop.CMS.epub").Length);
        }

        [Test]
        public void GetFilesToDelete_Epub_Mobi_CanDeleteOne()
        {
            var toDelete = GetFilesToDelete("Apress.Beginning.Backdrop.CMS.pdf", "Apress.Beginning.Backdrop.CMS.mobi");

            Assert.AreEqual(1, toDelete.Length);
            Assert.AreEqual("Apress.Beginning.Backdrop.CMS.mobi", toDelete[0]);
        }

        [Test]
        public void GetFilesToDelete_Mobi_Azw4_Azw3_CanDeleteTwo()
        {
            var toDelete = GetFilesToDelete("Apress.Beginning.Backdrop.CMS.mobi", 
                "Apress.Beginning.Backdrop.CMS.azw4",
                "Apress.Beginning.Backdrop.CMS.azw3");

            Assert.AreEqual(2, toDelete.Length);
            Assert.AreEqual("Apress.Beginning.Backdrop.CMS.azw4", toDelete[0]);
            Assert.AreEqual("Apress.Beginning.Backdrop.CMS.azw3", toDelete[1]);
        }

        private string[] GetFilesToDelete(params string[] fileNames)
        {
            var fileInfos = fileNames.Select(it => new FileInfo(it)).ToArray();
            return _feature.GetFilesToDelete(fileInfos)
                .Select(it => it.Name)
                .ToArray();
        }
    }
}
