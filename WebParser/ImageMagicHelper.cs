using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using ImageMagick;

namespace WebParser
{
    public static class ImageMagicHelper
    {

        public static async Task<bool> CheckImageExistsAndNotBroken(string imageUrl)
        {
            if (! await CheckImageExists(imageUrl))
            {
                return true; //ignore missing images at the moment
            }

            return CheckImageNotBroken(imageUrl);
        }
        
        private static bool CheckImageNotBroken(string imageUrl)
        {
            try
            {
                using (var image = new MagickImage(imageUrl))
                {
                    //$"convert {imageUrl} -gravity SouthWest -crop 20%x1%   -format %c  -depth 8  histogram:info:- | sed \'/^$/d\'  | sort -V | head -n 1 | grep fractal | wc -l"
                    image.Crop(new MagickGeometry(new Percentage(100), new Percentage(20)), Gravity.South);
                    image.Format = MagickFormat.C;
                    image.Depth = 8;

                    var histogram = image.Histogram();
                    Console.WriteLine($"Checking {imageUrl}, " +
                                      $"valid = {!(histogram.Count == 1 && histogram.First().Key == MagickColor.FromRgb(128, 128, 128))}");
                    return !(histogram.Count == 1 && histogram.First().Key == MagickColor.FromRgb(128, 128, 128));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception thrown for {imageUrl}" + ex.Message);
                return false;
            }
        }
        private static async Task<bool> CheckImageExists(string imageUrl)
        {
            WebResponse response = null;
            var request = (HttpWebRequest) WebRequest.Create(imageUrl);
            request.Method = "HEAD";
            var result = false;

            try
            {
                response = await request.GetResponseAsync();
                result = true;
            }
            catch (WebException ex)
            {
                Console.WriteLine($"error loading {imageUrl} : {ex.Message}");
            }
            finally
            {
                response?.Close();
            }

            Console.WriteLine($"check if image exists {imageUrl} = {result}");
            return result;
        }
    }
}