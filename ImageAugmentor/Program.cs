using System.Drawing;
using System.Drawing.Imaging;

var inputDir = "C:\\Users\\jbvbr\\OneDrive\\Skrivbord\\no_wildboar";
var outputDir = "C:\\Users\\jbvbr\\OneDrive\\Skrivbord\\augmented_no_wildboar";

Directory.CreateDirectory(outputDir);

var originalFiles = Directory.GetFiles(inputDir);
int originalCount = originalFiles.Length;
int targetCount = 120;

if (originalCount == 0)
{
    Console.WriteLine("Inga originalbilder hittades.");
    return;
}

int variantsPerImage = (int)Math.Ceiling((double)targetCount / originalCount);
Console.WriteLine($"Hittade {originalCount} originalbilder.");
Console.WriteLine($"Genererar {variantsPerImage} varianter per bild för att nå {targetCount} totalt.");

int created = 0;
int imageIndex = 0;

foreach (var file in originalFiles)
{
    using var original = new Bitmap(file);
    string baseName = Path.GetFileNameWithoutExtension(file);

    var variants = GenerateVariants(original, variantsPerImage);

    foreach (var variant in variants)
    {
        string outputPath = Path.Combine(outputDir, $"{baseName}_aug_{imageIndex++}.jpg");
        variant.Save(outputPath, ImageFormat.Jpeg);
        variant.Dispose();
        created++;
        if (created >= targetCount)
        {
            Console.WriteLine($"Klar – skapade {created} bilder.");
            return;
        }
    }
}

static List<Bitmap> GenerateVariants(Bitmap original, int count)
{
    var variants = new List<Bitmap>();

    for (int i = 0; i < count; i++)
    {
        Bitmap copy = new Bitmap(original);

        switch (i % 6)
        {
            case 0:
                variants.Add(copy); break; // Original
            case 1:
                copy.RotateFlip(RotateFlipType.RotateNoneFlipX);
                variants.Add(copy); break;
            case 2:
                variants.Add(RotateImage(copy, 10)); break;
            case 3:
                variants.Add(AdjustBrightness(copy, 1.2f)); break;
            case 4:
                variants.Add(AdjustBrightness(copy, 0.8f)); break;
            case 5:
                variants.Add(AddNoise(copy, 15)); break;
        }
    }

    return variants;
}

static Bitmap RotateImage(Bitmap image, float angle)
{
    var rotated = new Bitmap(image.Width, image.Height);
    using var g = Graphics.FromImage(rotated);
    g.TranslateTransform(image.Width / 2f, image.Height / 2f);
    g.RotateTransform(angle);
    g.TranslateTransform(-image.Width / 2f, -image.Height / 2f);
    g.DrawImage(image, new Point(0, 0));
    return rotated;
}

static Bitmap AdjustBrightness(Bitmap image, float factor)
{
    var adjusted = new Bitmap(image.Width, image.Height);
    using var g = Graphics.FromImage(adjusted);
    float[][] ptsArray = {
        new float[] { factor, 0, 0, 0, 0 },
        new float[] { 0, factor, 0, 0, 0 },
        new float[] { 0, 0, factor, 0, 0 },
        new float[] { 0, 0, 0, 1f, 0 },
        new float[] { 0, 0, 0, 0, 1f }
    };
    var matrix = new System.Drawing.Imaging.ColorMatrix(ptsArray);
    var attributes = new System.Drawing.Imaging.ImageAttributes();
    attributes.SetColorMatrix(matrix);
    g.DrawImage(image, new Rectangle(0, 0, image.Width, image.Height),
        0, 0, image.Width, image.Height, GraphicsUnit.Pixel, attributes);
    return adjusted;
}

static Bitmap AddNoise(Bitmap image, int noiseLevel)
{
    var random = new Random();
    var noisy = new Bitmap(image);

    for (int y = 0; y < noisy.Height; y++)
    {
        for (int x = 0; x < noisy.Width; x++)
        {
            var pixel = noisy.GetPixel(x, y);
            int r = Clamp(pixel.R + random.Next(-noiseLevel, noiseLevel));
            int g = Clamp(pixel.G + random.Next(-noiseLevel, noiseLevel));
            int b = Clamp(pixel.B + random.Next(-noiseLevel, noiseLevel));
            noisy.SetPixel(x, y, Color.FromArgb(r, g, b));
        }
    }
    return noisy;
}

static int Clamp(int value) => Math.Min(255, Math.Max(0, value));
