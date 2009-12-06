using System;
using NUnit.Framework;
using TagLib;
using TagLib.IFD;
using TagLib.IFD.Entries;
using TagLib.Jpeg;
using TagLib.Xmp;

namespace TagLib.Tests.Images
{
	[TestFixture]
	public class JpegPanasonicTest
	{
		private static string sample_file = "samples/sample_panasonic.jpg";
		private static string tmp_file = "samples/tmpwrite_panasonic.jpg";

		private TagTypes contained_types = TagTypes.TiffIFD | TagTypes.XMP;

		private File file;

		[TestFixtureSetUp]
		public void Init () {
			file = File.Create (sample_file);
		}

		[Test]
		public void JpegRead () {
			CheckTags (file);
		}

		[Test]
		public void ExifRead () {
			CheckExif (file);
		}

		[Test]
		public void MakernoteRead () {
			CheckMakerNote (file);
		}

		[Test]
		public void Rewrite () {
			File tmp = Utils.CreateTmpFile (sample_file, tmp_file);
			tmp.Save ();

			tmp = File.Create (tmp_file);

			CheckTags (tmp);
			CheckExif (tmp);
			CheckMakerNote (tmp);
		}

		[Test]
		public void AddExif ()
		{
			AddImageMetadataTests.AddExifTest (sample_file, tmp_file, true);
		}

		[Test]
		public void AddGPS ()
		{
			AddImageMetadataTests.AddGPSTest (sample_file, tmp_file, true);
		}

		[Test]
		public void AddXMP1 ()
		{
			AddImageMetadataTests.AddXMPTest1 (sample_file, tmp_file, true);
		}

		[Test]
		public void AddXMP2 ()
		{
			AddImageMetadataTests.AddXMPTest2 (sample_file, tmp_file, true);
		}

		public void CheckTags (File file) {
			Assert.IsTrue (file is Jpeg.File, "not a Jpeg file");

			Assert.AreEqual (contained_types, file.TagTypes);
			Assert.AreEqual (contained_types, file.TagTypesOnDisk);
		}

		public void CheckExif (File file) {
			var tag = file.GetTag (TagTypes.TiffIFD) as IFDTag;
			Assert.IsNotNull (tag, "Tiff Tag not contained");

			var exif_ifd = tag.Structure.GetEntry(0, IFDEntryTag.ExifIFD) as SubIFDEntry;
			Assert.IsNotNull (exif_ifd, "Exif SubIFD not contained");

			Assert.AreEqual ("Panasonic", tag.Make);
			Assert.AreEqual ("DMC-FX35", tag.Model);
			Assert.AreEqual (100, tag.ISOSpeedRatings);
			Assert.AreEqual (1.0d/80.0d, tag.ExposureTime);
			Assert.AreEqual (2.8d, tag.FNumber);
			Assert.AreEqual (4.4d, tag.FocalLength);
			Assert.AreEqual (new DateTime (2009, 06, 26, 12, 58, 30), tag.DateTime);
			Assert.AreEqual (new DateTime (2009, 06, 26, 14, 58, 30), tag.DateTimeDigitized);
			Assert.AreEqual (new DateTime (2009, 06, 26, 12, 58, 30), tag.DateTimeOriginal);
		}


		public void CheckMakerNote (File file) {
						IFDTag tag = file.GetTag (TagTypes.TiffIFD) as IFDTag;
			Assert.IsNotNull (tag, "Tiff Tag not contained");

			var makernote_ifd =
				tag.ExifIFD.GetEntry (0, (ushort) ExifEntryTag.MakerNote) as MakernoteIFDEntry;

			Assert.IsNotNull (makernote_ifd, "Makernote SubIFD not contained");
			Assert.AreEqual (MakernoteType.Panasonic, makernote_ifd.MakernoteType);

			var structure = makernote_ifd.Structure;
			Assert.IsNotNull (structure, "Makernote IFD Structure not contained");

			{
				var entry = structure.GetEntry (0, 0x01) as ShortIFDEntry;
				Assert.IsNotNull (entry, "entry 0x01");
				Assert.AreEqual (2, entry.Value);
			}
			{
				var entry = structure.GetEntry (0, 0x03) as ShortIFDEntry;
				Assert.IsNotNull (entry, "entry 0x03");
				Assert.AreEqual (1, entry.Value);
			}
			{
				var entry = structure.GetEntry (0, 0x07) as ShortIFDEntry;
				Assert.IsNotNull (entry, "entry 0x07");
				Assert.AreEqual (1, entry.Value);
			}
			{
				var entry = structure.GetEntry (0, 0x29) as LongIFDEntry;
				Assert.IsNotNull (entry, "entry 0x29");
				Assert.AreEqual (2286, entry.Value);
			}
			{
				var entry = structure.GetEntry (0, 0x26) as UndefinedIFDEntry;
				Assert.IsNotNull (entry, "entry 0x26");
				ByteVector read_bytes = entry.Data;
				ByteVector expected_bytes = new ByteVector (new byte [] {48, 50, 54, 48});

				Assert.AreEqual (expected_bytes.Count, read_bytes.Count);
				for (int i = 0; i < expected_bytes.Count; i++)
					Assert.AreEqual (expected_bytes[i], read_bytes[i]);
			}
		}
	}
}
