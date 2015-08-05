using System.IO;
using NUnit.Framework;

namespace fNbt.Test {
    [TestFixture]
    public sealed class TagSelectorTests {
        [Test]
        public void SkippingTagsOnFileLoad() {
            var stream = File.OpenRead( "TestFiles/bigtest.nbt" );
            NbtFile loadedFile = new NbtFile();
            loadedFile.LoadFromStream( stream,
                                     NbtCompression.AutoDetect,
                                     tag => tag.Name != "nested compound test" );
            Assert.IsFalse( loadedFile.RootTag.Contains( "nested compound test" ) );
            Assert.IsTrue( loadedFile.RootTag.Contains( "listTest (long)" ) );
            stream.Seek( 0, SeekOrigin.Begin );

            loadedFile.LoadFromStream( stream,
                                     NbtCompression.AutoDetect,
                                     tag => tag.TagType != NbtTagType.Float || tag.Parent.Name != "Level" );
            Assert.IsFalse( loadedFile.RootTag.Contains( "floatTest" ) );
            Assert.AreEqual( loadedFile.RootTag["nested compound test"]["ham"]["value"].FloatValue, 0.75 );
            stream.Seek(0, SeekOrigin.Begin);

            loadedFile.LoadFromStream( stream,
                                     NbtCompression.AutoDetect,
                                     tag => tag.Name != "listTest (long)" );
            Assert.IsFalse( loadedFile.RootTag.Contains( "listTest (long)" ) );
            Assert.IsTrue( loadedFile.RootTag.Contains( "byteTest" ) );
        }
    }
}