using ImageMagick;
using System;
using System.Collections.Generic;
using System.IO;

namespace OCR_Processor.Controllers
{
	public static class PDFToImageConverter
	{
		/// <summary>
		/// Converts a PDF to images optimized for OCR and returns the paths of the generated images.
		/// </summary>
		/// <param name="pdfPath">The path to the PDF file.</param>
		/// <param name="outputDirectory">The directory to save the images. If null, images will be saved in the current directory.</param>
		/// <param name="imageFormat">The image format (e.g., "png", "jpeg").</param>
		/// <param name="dpi">The DPI (dots per inch) for rendering the images.</param>
		/// <returns>A list of file paths to the generated images.</returns>
		public static List<string> ConvertPdfToImages(string pdfPath, string imageFormat = "tiff", int dpi = 300)
		{
			if (!File.Exists(pdfPath))
			{
				throw new FileNotFoundException("The specified PDF file was not found.", pdfPath);
			}

			Console.WriteLine($"Converting {pdfPath} to images...");
			var imagePaths = new List<string>();

			try
			{
				using (var images = new MagickImageCollection())
				{
					// Set the density (DPI) for better quality
					var readSettings = new MagickReadSettings
					{
						Density = new Density(dpi)
					};
					try
					{
						images.Read(pdfPath, readSettings);

					}
					catch (Exception ex)
					{
						Console.WriteLine($"Unable to reaad PDF: {ex.Message}");
					}
					// Read the PDF into the collection

					Console.WriteLine($"Number of pages: {images.Count}");

					int pageNumber = 1;
					foreach (MagickImage image in images)
					{
						// Optimize the image for OCR
						PreprocessImageForOCR(image);

						// Set the image format
						image.Format = Enum.TryParse(imageFormat, true, out MagickFormat format)
							? format
							: MagickFormat.Tiff;

						// Construct the output file path
						string outputFilePath = $"temp_page_{pageNumber}.{imageFormat}";
						image.Write(outputFilePath);

						imagePaths.Add(outputFilePath);
						pageNumber++;
					}
				}
			}
			catch (Exception ex)
			{
				throw new Exception("An error occurred while converting the PDF to images.", ex);
			}

			return imagePaths;
		}

		/// <summary>
		/// Preprocesses an image to optimize it for OCR.
		/// </summary>
		/// <param name="image">The image to preprocess.</param>
		private static void PreprocessImageForOCR(MagickImage image)
		{
			// Convert to grayscale
			image.ColorType = ColorType.Grayscale;

			// Enhance contrast
			image.Contrast();

			// Reduce noise
			image.ReduceNoise();

			// Apply adaptive thresholding for binarization
			image.Threshold(new Percentage(50)); // Adjust the percentage for better results
		}
	}
}