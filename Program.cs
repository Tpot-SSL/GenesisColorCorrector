using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace GenesisColorCorrector {
    class Program {
        // Base palette index.
        public static byte      BaseIndex       = 34;
        public static bool      UseBaseIndex    = false;
        public static byte[]    HardwareColors  = new byte[]{ 0, 52, 87, 116, 144, 172, 206, 255};

        public static byte NearestColor(byte color) {
            // Darkest Shade (0)
            if(color < 14)
               return 0;

            // (34)
            if(color < 50)
               return UseBaseIndex ? BaseIndex : HardwareColors[1];

            // (68)
            if(color < 80)
               return UseBaseIndex ? (byte)(BaseIndex * 2) : HardwareColors[2];

            // (102)
            if(color < 120)
               return UseBaseIndex ? (byte)(BaseIndex * 3) : HardwareColors[3];

            // (136)
            if(color < 150)
               return UseBaseIndex ? (byte)(BaseIndex * 4) : HardwareColors[4];

            // (170)
            if(color < 184)
               return UseBaseIndex ? (byte)(BaseIndex * 5) : HardwareColors[5];

            // (204)
            if(color < 220)
               return UseBaseIndex ? (byte)(BaseIndex * 6) : HardwareColors[6];

            // Lightest shade (238)
            return    UseBaseIndex ? (byte)(BaseIndex * 7) : HardwareColors[7];
        }

        static void Main(string[] args){
            // Create output directory.
            Directory.CreateDirectory(Environment.CurrentDirectory + "\\output\\");

            // Get files in current directory.
            string[] files = args.Length == 0 ? Directory.GetFiles(Environment.CurrentDirectory) : args;
            
            // Cycle through files.
            for(int index = 0; index < files.Length; index++) {
                string file = files[index];

                // Continue is file doesn't exist.
                if(!File.Exists(file))
                    continue;

                // If file isn't an image, skip it.
                if(Path.GetExtension(file) != ".png" && Path.GetExtension(file) != ".bmp" && Path.GetExtension(file) != ".gif" && Path.GetExtension(file) != ".jpg" && Path.GetExtension(file) != ".jpeg")
                    continue;

                // Load image
                Bitmap      image       = new Bitmap(file);

                // Set up for modifying pixels.
                BitmapData  pixelData   = image.LockBits(new Rectangle(new Point(0, 0), image.Size), ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);

                unsafe {
                    byte* srcPointer = (byte*)pixelData.Scan0;

                    // Cycle through pixels
                    for(int i = 0; i < image.Size.Height; i++) {
                        for(int j = 0; j < image.Size.Width; j++) {

                            // Replace each color channel with the closest match.
                            srcPointer[0] = NearestColor(srcPointer[0]); // Blue
                            srcPointer[1] = NearestColor(srcPointer[1]); // Green
                            srcPointer[2] = NearestColor(srcPointer[2]); // Red

                            // Next pixel (ignore alpha)
                            srcPointer += 4;
                        }
                    }
                }

                // Finalize Image
                image.UnlockBits(pixelData);

                // Save image
                using(MemoryStream memory = new MemoryStream()) {
                    using(FileStream fs = new FileStream(Environment.CurrentDirectory + "\\output\\" + Path.GetFileName(file), FileMode.Create, FileAccess.ReadWrite)) {
                        image.Save(memory, ImageFormat.Png);
                        byte[] bytes = memory.ToArray();
                        fs.Write(bytes, 0, bytes.Length);
                    }
                }
            }
        }
    }
}
