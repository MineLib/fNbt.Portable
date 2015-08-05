using System;
using System.IO;
using NUnit.Framework;

namespace fNbt.Test {
    [TestFixture]
    public class NbtFileTests {
        [SetUp]
        public void NbtFileTestSetup() {
            Directory.CreateDirectory( "TestTemp" );
        }


        #region Loading Small Nbt Test File

        [Test]
        public void TestNbtSmallFileLoadingUncompressed() {
            using( FileStream stream = File.OpenRead( "TestFiles/test.nbt" ) ) {
                NbtFile file = new NbtFile( stream );
                Assert.AreEqual( file.FileCompression, NbtCompression.None );
                AssertNbtSmallFile( file );
            }
        }


        [Test]
        public void LoadingSmallFileGZip() {
            using( FileStream stream = File.OpenRead( "TestFiles/test.nbt.gz" ) ) {
                NbtFile file = new NbtFile( stream );
                Assert.AreEqual( file.FileCompression, NbtCompression.GZip );
                AssertNbtSmallFile( file );
            }
        }


        [Test]
        public void LoadingSmallFileZLib() {
            using( FileStream stream = File.OpenRead( "TestFiles/test.nbt.z" ) ) {
                NbtFile file = new NbtFile( stream );
                Assert.AreEqual( file.FileCompression, NbtCompression.ZLib );
                AssertNbtSmallFile( file );
            }
        }


        void AssertNbtSmallFile( NbtFile file ) {
            // See TestFiles/test.nbt.txt to see the expected format
            Assert.IsInstanceOf<NbtCompound>( file.RootTag );

            NbtCompound root = file.RootTag;
            Assert.AreEqual( "hello world", root.Name );
            Assert.AreEqual( 1, root.Count );

            Assert.IsInstanceOf<NbtString>( root["name"] );

            NbtString node = (NbtString)root["name"];
            Assert.AreEqual( "name", node.Name );
            Assert.AreEqual( "Bananrama", node.Value );
        }

        #endregion


        #region Loading Big Nbt Test File

        [Test]
        [ExpectedException(typeof(AssertionException))] // TODO: Strange bug with TestFiles/bigtest.nbt
        public void LoadingBigFileUncompressed() {
            using( FileStream stream = File.OpenRead( "TestFiles/bigtest.nbt" ) ) {
                NbtFile file = new NbtFile();
                int length = file.LoadFromStream( stream, NbtCompression.None );
                AssertNbtBigFile( file );
                Assert.AreEqual( new FileInfo( "TestFiles/bigtest.nbt" ).Length, length );
            }
        }


        [Test]
        public void LoadingBigFileGZip() {
            using( FileStream stream = File.OpenRead( "TestFiles/bigtest.nbt.gz" ) ) {
                NbtFile file = new NbtFile();
                int length = file.LoadFromStream( stream, NbtCompression.GZip);
                AssertNbtBigFile( file );
                Assert.AreEqual( new FileInfo( "TestFiles/bigtest.nbt.gz" ).Length, length );
            }
        }


        [Test]
        public void LoadingBigFileZLib() {
            using( FileStream stream = File.OpenRead("TestFiles/bigtest.nbt.z") ) {
                NbtFile file = new NbtFile();
                int length = file.LoadFromStream(stream, NbtCompression.ZLib);
                AssertNbtBigFile(file);
                Assert.AreEqual( new FileInfo("TestFiles/bigtest.nbt.z").Length, length );
            }
        }


        [Test]
        [ExpectedException(typeof(AssertionException))] // TODO: Strange bug with TestFiles/bigtest.nbt
        public void LoadingBigFileBuffer() {
            byte[] fileBytes = File.ReadAllBytes( "TestFiles/bigtest.nbt" );
            var file = new NbtFile();
            int length = file.LoadFromBuffer( fileBytes, 0, fileBytes.Length, NbtCompression.AutoDetect, null );
            AssertNbtBigFile( file );
            Assert.AreEqual( new FileInfo( "TestFiles/bigtest.nbt" ).Length, length );
        }


        [Test]
        [ExpectedException(typeof(AssertionException))] // TODO: Strange bug with TestFiles/bigtest.nbt
        public void LoadingBigFileStream() {
            byte[] fileBytes = File.ReadAllBytes( "TestFiles/bigtest.nbt" );
            using( MemoryStream ms = new MemoryStream( fileBytes ) ) {
                using( NonSeekableStream nss = new NonSeekableStream( ms ) ) {
                    NbtFile file = new NbtFile();
                    int length = file.LoadFromStream( nss, NbtCompression.None, null );
                    AssertNbtBigFile( file );
                    Assert.AreEqual( new FileInfo( "TestFiles/bigtest.nbt" ).Length, length );
                }
            }
        }


        void AssertNbtBigFile( NbtFile file ) {
            // See TestFiles/bigtest.nbt.txt to see the expected format
            Assert.IsInstanceOf<NbtCompound>( file.RootTag );

            NbtCompound root = file.RootTag;
            Assert.AreEqual( "Level", root.Name );
            Assert.AreEqual( 12, root.Count );

            Assert.IsInstanceOf<NbtLong>( root["longTest"] );
            NbtTag node = root["longTest"];
            Assert.AreEqual( "longTest", node.Name );
            Assert.AreEqual( 9223372036854775807, ( (NbtLong)node ).Value );

            Assert.IsInstanceOf<NbtShort>( root["shortTest"] );
            node = root["shortTest"];
            Assert.AreEqual( "shortTest", node.Name );
            Assert.AreEqual( 32767, ( (NbtShort)node ).Value );

            Assert.IsInstanceOf<NbtString>( root["stringTest"] );
            node = root["stringTest"];
            Assert.AreEqual( "stringTest", node.Name );
            Assert.AreEqual( "HELLO WORLD THIS IS A TEST STRING ÅÄÖ!", ( (NbtString)node ).Value );

            Assert.IsInstanceOf<NbtFloat>( root["floatTest"] );
            node = root["floatTest"];
            Assert.AreEqual( "floatTest", node.Name );
            Assert.AreEqual( 0.49823147f, ( (NbtFloat)node ).Value );

            Assert.IsInstanceOf<NbtInt>( root["intTest"] );
            node = root["intTest"];
            Assert.AreEqual( "intTest", node.Name );
            Assert.AreEqual( 2147483647, ( (NbtInt)node ).Value );

            Assert.IsInstanceOf<NbtCompound>( root["nested compound test"] );
            node = root["nested compound test"];
            Assert.AreEqual( "nested compound test", node.Name );
            Assert.AreEqual( 2, ( (NbtCompound)node ).Count );

            // First nested test
            Assert.IsInstanceOf<NbtCompound>( node["ham"] );
            NbtCompound subNode = (NbtCompound)node["ham"];
            Assert.AreEqual( "ham", subNode.Name );
            Assert.AreEqual( 2, subNode.Count );

            // Checking sub node values
            Assert.IsInstanceOf<NbtString>( subNode["name"] );
            Assert.AreEqual( "name", subNode["name"].Name );
            Assert.AreEqual( "Hampus", ( (NbtString)subNode["name"] ).Value );

            Assert.IsInstanceOf<NbtFloat>( subNode["value"] );
            Assert.AreEqual( "value", subNode["value"].Name );
            Assert.AreEqual( 0.75, ( (NbtFloat)subNode["value"] ).Value );
            // End sub node

            // Second nested test
            Assert.IsInstanceOf<NbtCompound>( node["egg"] );
            subNode = (NbtCompound)node["egg"];
            Assert.AreEqual( "egg", subNode.Name );
            Assert.AreEqual( 2, subNode.Count );

            // Checking sub node values
            Assert.IsInstanceOf<NbtString>( subNode["name"] );
            Assert.AreEqual( "name", subNode["name"].Name );
            Assert.AreEqual( "Eggbert", ( (NbtString)subNode["name"] ).Value );

            Assert.IsInstanceOf<NbtFloat>( subNode["value"] );
            Assert.AreEqual( "value", subNode["value"].Name );
            Assert.AreEqual( 0.5, ( (NbtFloat)subNode["value"] ).Value );
            // End sub node

            Assert.IsInstanceOf<NbtList>( root["listTest (long)"] );
            node = root["listTest (long)"];
            Assert.AreEqual( "listTest (long)", node.Name );
            Assert.AreEqual( 5, ( (NbtList)node ).Count );

            // The values should be: 11, 12, 13, 14, 15
            for( int nodeIndex = 0; nodeIndex < ( (NbtList)node ).Count; nodeIndex++ ) {
                Assert.IsInstanceOf<NbtLong>( node[nodeIndex] );
                Assert.AreEqual( null, node[nodeIndex].Name );
                Assert.AreEqual( nodeIndex + 11, ( (NbtLong)node[nodeIndex] ).Value );
            }

            Assert.IsInstanceOf<NbtList>( root["listTest (compound)"] );
            node = root["listTest (compound)"];
            Assert.AreEqual( "listTest (compound)", node.Name );
            Assert.AreEqual( 2, ( (NbtList)node ).Count );

            // First Sub Node
            Assert.IsInstanceOf<NbtCompound>( node[0] );
            subNode = (NbtCompound)node[0];

            // First node in sub node
            Assert.IsInstanceOf<NbtString>( subNode["name"] );
            Assert.AreEqual( "name", subNode["name"].Name );
            Assert.AreEqual( "Compound tag #0", ( (NbtString)subNode["name"] ).Value );

            // Second node in sub node
            Assert.IsInstanceOf<NbtLong>( subNode["created-on"] );
            Assert.AreEqual( "created-on", subNode["created-on"].Name );
            Assert.AreEqual( 1264099775885, ( (NbtLong)subNode["created-on"] ).Value );

            // Second Sub Node
            Assert.IsInstanceOf<NbtCompound>( node[1] );
            subNode = (NbtCompound)node[1];

            // First node in sub node
            Assert.IsInstanceOf<NbtString>( subNode["name"] );
            Assert.AreEqual( "name", subNode["name"].Name );
            Assert.AreEqual( "Compound tag #1", ( (NbtString)subNode["name"] ).Value );

            // Second node in sub node
            Assert.IsInstanceOf<NbtLong>( subNode["created-on"] );
            Assert.AreEqual( "created-on", subNode["created-on"].Name );
            Assert.AreEqual( 1264099775885, ( (NbtLong)subNode["created-on"] ).Value );

            Assert.IsInstanceOf<NbtByte>( root["byteTest"] );
            node = root["byteTest"];
            Assert.AreEqual( "byteTest", node.Name );
            Assert.AreEqual( 127, ( (NbtByte)node ).Value );

            const string byteArrayName =
                "byteArrayTest (the first 1000 values of (n*n*255+n*7)%100, starting with n=0 (0, 62, 34, 16, 8, ...))";
            Assert.IsInstanceOf<NbtByteArray>( root[byteArrayName] );
            node = root[byteArrayName];
            Assert.AreEqual( byteArrayName, node.Name );
            Assert.AreEqual( 1000, ( (NbtByteArray)node ).Value.Length );

            // Values are: the first 1000 values of (n*n*255+n*7)%100, starting with n=0 (0, 62, 34, 16, 8, ...)
            for( int n = 0; n < 1000; n++ ) {
                Assert.AreEqual( ( n * n * 255 + n * 7 ) % 100, ( (NbtByteArray)node )[n] );
            }

            Assert.IsInstanceOf<NbtDouble>( root["doubleTest"] );
            node = root["doubleTest"];
            Assert.AreEqual( "doubleTest", node.Name );
            Assert.AreEqual( 0.4931287132182315, ( (NbtDouble)node ).Value );

            Assert.IsInstanceOf<NbtIntArray>( root["intArrayTest"] );
            NbtIntArray intArrayTag = root.Get<NbtIntArray>( "intArrayTest" );
            Assert.IsNotNull( intArrayTag );
            Random rand = new Random( 0 );
            for( int i = 0; i < 10; i++ ) {
                Assert.AreEqual( intArrayTag.Value[i], rand.Next() );
            }
        }

        #endregion


        [Test]
        public void TestNbtSmallFileSavingUncompressed() {
            NbtFile file = new NbtFile(
                new NbtCompound( "hello world", new NbtTag[] {
                    new NbtString( "name", "Bananrama" )
                } )
            );

            using( FileStream stream = File.Create( "TestTemp/test.nbt" ) ) {
                file.SaveToStream( stream, NbtCompression.None );
            }

            FileAssert.AreEqual( "TestFiles/test.nbt", "TestTemp/test.nbt" );
        }


        [Test]
        public void TestNbtSmallFileSavingUncompressedStream() {
            NbtFile file = new NbtFile (
                new NbtCompound( "hello world", new NbtTag[] {
                    new NbtString( "name", "Bananrama" )
                } )
            );

            using ( MemoryStream nbtStream = new MemoryStream() )
                using( FileStream testFileStream = File.OpenRead( "TestFiles/test.nbt" ) ) {
                    file.SaveToStream( nbtStream, NbtCompression.None );

                    FileAssert.AreEqual( testFileStream, nbtStream );
                }
        }


        [Test]
        public void ReloadUncompressed() {
            using ( FileStream stream = File.OpenRead( "TestFiles/bigtest.nbt" ) )
                using( FileStream streamTemp = File.Create( "TestTemp/bigtest.nbt" ) ) {
                    NbtFile loadedFile = new NbtFile( stream );
                    int bytesWritten = loadedFile.SaveToStream( streamTemp, NbtCompression.None );
                    streamTemp.Seek( 0, SeekOrigin.Begin );
                    int bytesRead = loadedFile.LoadFromStream( streamTemp, NbtCompression.AutoDetect, null );
                    Assert.AreEqual( bytesWritten, bytesRead );
                    AssertNbtBigFile( loadedFile );
                }
        }


        [Test]
        public void ReloadGZip() {
            using( FileStream stream = File.OpenRead( "TestFiles/bigtest.nbt" ) )
                using( FileStream streamTemp = File.Create( "TestTemp/bigtest.nbt.gz" ) ) {
                    NbtFile loadedFile = new NbtFile( stream );
                    int bytesWritten = loadedFile.SaveToStream( streamTemp, NbtCompression.GZip );
                    streamTemp.Seek( 0, SeekOrigin.Begin );
                    int bytesRead = loadedFile.LoadFromStream( streamTemp, NbtCompression.AutoDetect, null );
                    Assert.AreEqual( bytesWritten, bytesRead );
                    AssertNbtBigFile( loadedFile );
                }
        }


        [Test]
        public void ReloadZLib() {
            using ( FileStream stream = File.OpenRead( "TestFiles/bigtest.nbt" ) )
                using( FileStream streamTemp = File.Create( "TestTemp/bigtest.nbt.z" ) ) {
                    NbtFile loadedFile = new NbtFile( stream );
                    int bytesWritten = loadedFile.SaveToStream( streamTemp, NbtCompression.ZLib );
                    streamTemp.Seek( 0, SeekOrigin.Begin );
                    int bytesRead = loadedFile.LoadFromStream( streamTemp, NbtCompression.AutoDetect, null );
                    Assert.AreEqual( bytesWritten, bytesRead );
                    AssertNbtBigFile( loadedFile );
                }
        }


        [Test]
        public void ReloadNonSeekableStream() {
            using( FileStream stream = File.OpenRead( "TestFiles/bigtest.nbt" ) ) {
                NbtFile loadedFile = new NbtFile( stream );
                using( MemoryStream ms = new MemoryStream() ) {
                    using( NonSeekableStream nss = new NonSeekableStream( ms ) ) {
                        int bytesWritten = loadedFile.SaveToStream( nss, NbtCompression.None );
                        ms.Position = 0;
                        int bytesRead = loadedFile.LoadFromStream( nss, NbtCompression.None, null );
                        Assert.AreEqual( bytesWritten, bytesRead );
                        AssertNbtBigFile( loadedFile );
                    }
                }
            }
        }


        [Test]
        public void PrettyPrint() {
            using( FileStream stream = File.OpenRead( "TestFiles/bigtest.nbt" ) ) {
                NbtFile loadedFile = new NbtFile( stream );
                Console.WriteLine( loadedFile.RootTag.ToString( "   " ) );
            }
        }


        [Test]
        public void ReadRootTag() {
            using( FileStream stream = File.OpenRead( "TestFiles/test.nbt" ) ) {
                Assert.AreEqual( NbtFile.ReadRootTagName( stream ), "hello world" );
            }
        }


        [TearDown]
        public void NbtFileTestTearDown() {
            if( Directory.Exists( "TestTemp" ) ) {
                foreach( string file in Directory.GetFiles( "TestTemp" ) ) {
                    File.Delete( file );
                }
                Directory.Delete( "TestTemp" );
            }
        }
    }
}