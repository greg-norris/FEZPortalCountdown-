using System.Drawing;
using System.Threading;
using GHIElectronics.TinyCLR.Devices.Display;
using GHIElectronics.TinyCLR.Devices.Gpio;
using GHIElectronics.TinyCLR.Pins;
using GHIElectronics.TinyCLR.Drivers.FocalTech.FT5xx6;
using GHIElectronics.TinyCLR.Devices.I2c;

using GHIElectronics.TinyCLR.Devices.Pwm;
using System.Diagnostics;

namespace FEZPortalClock
{
    class Program
    {

        static int sec = 59;
        static int min = 1;
        static int hour = 0;
        static int y = 90;
        static int x = 20;
        static int color = 3;//RBGY
        static FT5xx6Controller touch;
        static Bitmap[] digits = new Bitmap[11];
        static bool timeleft = true;
        //RED LED Images
        static Bitmap LED0 = Resources.GetBitmap(Resources.BitmapResources.LED0);
        static Bitmap LED1R = Resources.GetBitmap(Resources.BitmapResources.LED1R);
        static Bitmap LED2R = Resources.GetBitmap(Resources.BitmapResources.LED2R);
        static Bitmap LED3R = Resources.GetBitmap(Resources.BitmapResources.LED3R);
        static Bitmap LED4R = Resources.GetBitmap(Resources.BitmapResources.LED4R);
        static Bitmap LED5R = Resources.GetBitmap(Resources.BitmapResources.LED5R);
        static Bitmap LED6R = Resources.GetBitmap(Resources.BitmapResources.LED6R);
        static Bitmap LED7R = Resources.GetBitmap(Resources.BitmapResources.LED7R);
        static Bitmap LED8R = Resources.GetBitmap(Resources.BitmapResources.LED8R);
        static Bitmap LED9R = Resources.GetBitmap(Resources.BitmapResources.LED9R);
        static Bitmap LED = Resources.GetBitmap(Resources.BitmapResources.LED);
        //GREEN LED Images
        static Bitmap LED0G = Resources.GetBitmap(Resources.BitmapResources.LED0G);
        static Bitmap LED1G = Resources.GetBitmap(Resources.BitmapResources.LED1G);
        static Bitmap LED2G = Resources.GetBitmap(Resources.BitmapResources.LED2G);
        static Bitmap LED3G = Resources.GetBitmap(Resources.BitmapResources.LED3G);
        static Bitmap LED4G = Resources.GetBitmap(Resources.BitmapResources.LED4G);
        static Bitmap LED5G = Resources.GetBitmap(Resources.BitmapResources.LED5G);
        static Bitmap LED6G = Resources.GetBitmap(Resources.BitmapResources.LED6G);
        static Bitmap LED7G = Resources.GetBitmap(Resources.BitmapResources.LED7G);
        static Bitmap LED8G = Resources.GetBitmap(Resources.BitmapResources.LED8G);
        static Bitmap LED9G = Resources.GetBitmap(Resources.BitmapResources.LED9G);
        static Bitmap LEDG = Resources.GetBitmap(Resources.BitmapResources.LEDG);
        //BLUE LED Images
        static Bitmap LED0B = Resources.GetBitmap(Resources.BitmapResources.LED0B);
        static Bitmap LED1B = Resources.GetBitmap(Resources.BitmapResources.LED1B);
        static Bitmap LED2B = Resources.GetBitmap(Resources.BitmapResources.LED2B);
        static Bitmap LED3B = Resources.GetBitmap(Resources.BitmapResources.LED3B);
        static Bitmap LED4B = Resources.GetBitmap(Resources.BitmapResources.LED4B);
        static Bitmap LED5B = Resources.GetBitmap(Resources.BitmapResources.LED5B);
        static Bitmap LED6B = Resources.GetBitmap(Resources.BitmapResources.LED6B);
        static Bitmap LED7B = Resources.GetBitmap(Resources.BitmapResources.LED7B);
        static Bitmap LED8B = Resources.GetBitmap(Resources.BitmapResources.LED8B);
        static Bitmap LED9B = Resources.GetBitmap(Resources.BitmapResources.LED9B);
        static Bitmap LEDB = Resources.GetBitmap(Resources.BitmapResources.LEDB);

        static Bitmap[] blue = new Bitmap[] { LED0B, LED1B, LED2B, LED3B, LED4B, LED5B, LED6B, LED7B, LED8B, LED9B, LEDB };
        static Bitmap[] red = new Bitmap[] { LED0, LED1R, LED2R, LED3R, LED4R, LED5R, LED6R, LED7R, LED8R, LED9R, LED };
        static Bitmap[] green = new Bitmap[] { LED0G, LED1G, LED2G, LED3G, LED4G, LED5G, LED6G, LED7G, LED8G, LED9G, LEDG };

        static void Main()
        {


            var i2cController = I2cController.FromName(SC20260.I2cBus.I2c1);

            var settings = new I2cConnectionSettings(0x38)
            {
                BusSpeed = 100000,
                AddressFormat = I2cAddressFormat.SevenBit,
            };

            var i2cDevice = i2cController.GetDevice(settings);

            var gpioController = GpioController.GetDefault();
            var interrupt = gpioController.OpenPin(SC20260.GpioPin.PG9);

            touch = new FT5xx6Controller(i2cDevice, interrupt);
            touch.TouchDown += Touch_TouchDown;
            touch.TouchUp += Touch_TouchUp;

            GpioPin backlight = GpioController.GetDefault().OpenPin(SC20260.GpioPin.PA15);
            backlight.SetDriveMode(GpioPinDriveMode.Output);
            backlight.Write(GpioPinValue.High);

            var displayController = DisplayController.GetDefault();
            // Enter the proper display configurations
            displayController.SetConfiguration(new ParallelDisplayControllerSettings
            {
                Width = 480,
                Height = 272,
                DataFormat = DisplayDataFormat.Rgb565,
                Orientation = DisplayOrientation.Degrees0, //Rotate display.
                PixelClockRate = 10000000,
                PixelPolarity = false,
                DataEnablePolarity = false,
                DataEnableIsFixed = false,
                HorizontalFrontPorch = 2,
                HorizontalBackPorch = 2,
                HorizontalSyncPulseWidth = 41,
                HorizontalSyncPolarity = false,
                VerticalFrontPorch = 2,
                VerticalBackPorch = 2,
                VerticalSyncPulseWidth = 10,
                VerticalSyncPolarity = false,
            });

            displayController.Enable();

            var screen = Graphics.FromHdc(displayController.Hdc);

            //SET DISPLAY to DEFAULT COLOR GREEN
            for (x = 0; x < digits.Length; x++)
            {
                digits[x] = green[x];
            }

            //CountDown timer
            while (timeleft)
            {

                if (color == 1)
                {

                    for (x = 0; x < digits.Length; x++)
                    {
                        digits[x] = red[x];
                    }

                }
                else if (color == 2)
                {

                    for (x = 0; x < digits.Length; x++)
                    {
                        digits[x] = blue[x];
                    }
                }
                else if (color == 3)
                {

                    for (x = 0; x < digits.Length; x++)
                    {
                        digits[x] = green[x];
                    }
                }

                if (sec <= 0 && min <= 0 && hour <= 0)
                {
                    sec = 0;
                    min = 0;
                    hour = 0;
                    Sound_Alarm();
                    timeleft = false;
                }
                if (sec == 0 && min > 0 && hour > 0)
                {
                    sec = 60;
                    min--;
                }
                if (sec == 0 && min > 0 && hour == 0)
                {
                    sec = 60;
                    hour = 0;
                    min--;
                }
                if (sec == 0 && min == 0 && hour > 0)
                {
                    sec = 60;
                    min = 59;
                    hour--;
                }
                sec--;

                screen.Clear();

                switch (hour)
                {
                    case 0:
                        screen.DrawImage(digits[0], x + 0, y + 0);
                        screen.DrawImage(digits[0], x + 70, y + 0);
                        break;
                    case 1:
                        screen.DrawImage(digits[0], x + 0, y + 0);
                        screen.DrawImage(digits[1], x + 70, y + 0);
                        break;
                    case 2:
                        screen.DrawImage(digits[0], x + 0, y + 0);
                        screen.DrawImage(digits[2], x + 70, y + 0);
                        break;
                    case 3:
                        screen.DrawImage(digits[0], x + 0, y + 0);
                        screen.DrawImage(digits[3], x + 70, y + 0);
                        break;
                    case 4:
                        screen.DrawImage(digits[0], x + 0, y + 0);
                        screen.DrawImage(digits[4], x + 70, y + 0);
                        break;
                    case 5:
                        screen.DrawImage(digits[0], x + 0, y + 0);
                        screen.DrawImage(digits[5], x + 70, y + 0);
                        break;
                    case 6:
                        screen.DrawImage(digits[0], x + 0, y + 0);
                        screen.DrawImage(digits[6], x + 70, y + 0);
                        break;
                    case 7:
                        screen.DrawImage(digits[0], x + 0, y + 0);
                        screen.DrawImage(digits[7], x + 70, y + 0);
                        break;
                    case 8:
                        screen.DrawImage(digits[0], 10, y + 0);
                        screen.DrawImage(digits[8], x + 70, y + 0);
                        break;
                    case 9:
                        screen.DrawImage(digits[0], x + 0, y + 0);
                        screen.DrawImage(digits[9], x + 70, y + 0);
                        break;
                    case 10:
                        screen.DrawImage(digits[1], x + 0, y + 0);
                        screen.DrawImage(digits[0], x + 70, y + 0);
                        break;
                    case 11:
                        screen.DrawImage(digits[1], x + 0, y + 0);
                        screen.DrawImage(digits[1], x + 70, y + 0);
                        break;
                    case 12:
                        screen.DrawImage(digits[1], x + 0, y + 0);
                        screen.DrawImage(digits[2], x + 70, y + 0);
                        break;
                    case 13:
                        screen.DrawImage(digits[1], x + 0, y + 0);
                        screen.DrawImage(digits[3], x + 70, y + 0);
                        break;
                    case 14:
                        screen.DrawImage(digits[1], x + 0, y + 0);
                        screen.DrawImage(digits[4], x + 70, y + 0);
                        break;
                    case 15:
                        screen.DrawImage(digits[0], x + 0, y + 0);
                        screen.DrawImage(digits[5], x + 70, y + 0);
                        break;
                    case 16:
                        screen.DrawImage(digits[1], x + 0, y + 0);
                        screen.DrawImage(digits[6], x + 70, y + 0);
                        break;
                    case 17:
                        screen.DrawImage(digits[1], x + 0, y + 0);
                        screen.DrawImage(digits[7], x + 70, y + 0);
                        break;
                    case 18:
                        screen.DrawImage(digits[1], x + 0, y + 0);
                        screen.DrawImage(digits[8], x + 70, y + 0);
                        break;
                    case 19:
                        screen.DrawImage(digits[1], x + 0, y + 0);
                        screen.DrawImage(digits[9], x + 70, y + 0);
                        break;
                    case 20:
                        screen.DrawImage(digits[2], x + 0, y + 0);
                        screen.DrawImage(digits[0], x + 70, y + 0);
                        break;
                    case 21:
                        screen.DrawImage(digits[2], x + 0, y + 0);
                        screen.DrawImage(digits[1], x + 70, y + 0);
                        break;
                    case 22:
                        screen.DrawImage(digits[2], x + 0, y + 0);
                        screen.DrawImage(digits[2], x + 70, y + 0);
                        break;
                    case 23:
                        screen.DrawImage(digits[2], x + 0, y + 0);
                        screen.DrawImage(digits[3], x + 70, y + 0);
                        break;
                    default:
                        break;
                }

                switch (min)
                {
                    case 0:
                        screen.DrawImage(digits[10], x + 135, y + 22);
                        screen.DrawImage(digits[0], x + 155, y + 0);
                        screen.DrawImage(digits[0], x + 225, y + 0);
                        break;
                    case 1:
                        screen.DrawImage(digits[10], x + 135, y + 22);
                        screen.DrawImage(digits[0], x + 155, y + 0);
                        screen.DrawImage(digits[1], x + 225, y + 0);
                        break;
                    case 2:
                        screen.DrawImage(digits[10], x + 135, y + 22);
                        screen.DrawImage(digits[0], x + 155, y + 0);
                        screen.DrawImage(digits[2], x + 225, y + 0);
                        break;
                    case 3:
                        screen.DrawImage(digits[10], x + 135, y + 22);
                        screen.DrawImage(digits[0], x + 155, y + 0);
                        screen.DrawImage(digits[3], x + 225, y + 0);
                        break;
                    case 4:
                        screen.DrawImage(digits[10], x + 135, y + 22);
                        screen.DrawImage(digits[0], x + 155, y + 0);
                        screen.DrawImage(digits[4], x + 225, y + 0);
                        break;
                    case 5:
                        screen.DrawImage(digits[10], x + 135, y + 22);
                        screen.DrawImage(digits[0], x + 155, y + 0);
                        screen.DrawImage(digits[5], x + 225, y + 0);
                        break;
                    case 6:
                        screen.DrawImage(digits[10], x + 135, y + 22);
                        screen.DrawImage(digits[0], x + 155, y + 0);
                        screen.DrawImage(digits[6], x + 225, y + 0);
                        break;
                    case 7:
                        screen.DrawImage(digits[10], x + 135, y + 22);
                        screen.DrawImage(digits[0], x + 155, y + 0);
                        screen.DrawImage(digits[7], x + 225, y + 0);
                        break;
                    case 8:
                        screen.DrawImage(digits[10], x + 135, y + 22);
                        screen.DrawImage(digits[0], x + 155, y + 0);
                        screen.DrawImage(digits[8], x + 225, y + 0);
                        break;
                    case 9:
                        screen.DrawImage(digits[10], x + 135, y + 22);
                        screen.DrawImage(digits[0], x + 155, y + 0);
                        screen.DrawImage(digits[9], x + 225, y + 0);
                        break;
                    case 10:
                        screen.DrawImage(digits[10], x + 135, y + 22);
                        screen.DrawImage(digits[1], x + 155, y + 0);
                        screen.DrawImage(digits[0], x + 225, y + 0);
                        break;
                    case 11:
                        screen.DrawImage(digits[10], x + 135, y + 22);
                        screen.DrawImage(digits[1], x + 155, y + 0);
                        screen.DrawImage(digits[1], x + 225, y + 0);
                        break;
                    case 12:
                        screen.DrawImage(digits[10], x + 135, y + 22);
                        screen.DrawImage(digits[1], x + 155, y + 0);
                        screen.DrawImage(digits[2], x + 225, y + 0);
                        break;
                    case 13:
                        screen.DrawImage(digits[10], x + 135, y + 22);
                        screen.DrawImage(digits[1], x + 155, y + 0);
                        screen.DrawImage(digits[3], x + 225, y + 0);
                        break;
                    case 14:
                        screen.DrawImage(digits[10], x + 135, y + 22);
                        screen.DrawImage(digits[1], x + 155, y + 0);
                        screen.DrawImage(digits[4], x + 225, y + 0);
                        break;
                    case 15:
                        screen.DrawImage(digits[10], x + 135, y + 22);
                        screen.DrawImage(digits[1], x + 155, y + 0);
                        screen.DrawImage(digits[5], x + 225, y + 0);
                        break;
                    case 16:
                        screen.DrawImage(digits[10], x + 135, y + 22);
                        screen.DrawImage(digits[1], x + 155, y + 0);
                        screen.DrawImage(digits[6], x + 225, y + 0);
                        break;
                    case 17:
                        screen.DrawImage(digits[10], x + 135, y + 22);
                        screen.DrawImage(digits[1], x + 155, y + 0);
                        screen.DrawImage(digits[7], x + 225, y + 0);
                        break;
                    case 18:
                        screen.DrawImage(digits[10], x + 135, y + 22);
                        screen.DrawImage(digits[1], x + 155, y + 0);
                        screen.DrawImage(digits[8], x + 225, y + 0);
                        break;
                    case 19:
                        screen.DrawImage(digits[10], x + 135, y + 22);
                        screen.DrawImage(digits[1], x + 155, y + 0);
                        screen.DrawImage(digits[9], x + 225, y + 0);
                        break;
                    case 20:
                        screen.DrawImage(digits[10], x + 135, y + 22);
                        screen.DrawImage(digits[2], x + 155, y + 0);
                        screen.DrawImage(digits[0], x + 225, y + 0);
                        break;
                    case 21:
                        screen.DrawImage(digits[10], x + 135, y + 22);
                        screen.DrawImage(digits[2], x + 155, y + 0);
                        screen.DrawImage(digits[1], x + 225, y + 0);
                        break;
                    case 22:
                        screen.DrawImage(digits[10], x + 135, y + 22);
                        screen.DrawImage(digits[2], x + 155, y + 0);
                        screen.DrawImage(digits[2], x + 225, y + 0);
                        break;
                    case 23:
                        screen.DrawImage(digits[10], x + 135, y + 22);
                        screen.DrawImage(digits[2], x + 155, y + 0);
                        screen.DrawImage(digits[3], x + 225, y + 0);
                        break;
                    case 24:
                        screen.DrawImage(digits[10], x + 135, y + 22);
                        screen.DrawImage(digits[2], x + 155, y + 0);
                        screen.DrawImage(digits[4], x + 225, y + 0);
                        break;
                    case 25:
                        screen.DrawImage(digits[10], x + 135, y + 22);
                        screen.DrawImage(digits[2], x + 155, y + 0);
                        screen.DrawImage(digits[5], x + 225, y + 0);
                        break;
                    case 26:
                        screen.DrawImage(digits[10], x + 135, y + 22);
                        screen.DrawImage(digits[2], x + 155, y + 0);
                        screen.DrawImage(digits[6], x + 225, y + 0);
                        break;
                    case 27:
                        screen.DrawImage(digits[10], x + 135, y + 22);
                        screen.DrawImage(digits[2], x + 155, y + 0);
                        screen.DrawImage(digits[7], x + 225, y + 0);
                        break;
                    case 28:
                        screen.DrawImage(digits[10], x + 135, y + 22);
                        screen.DrawImage(digits[2], x + 155, y + 0);
                        screen.DrawImage(digits[8], x + 225, y + 0);
                        break;
                    case 29:
                        screen.DrawImage(digits[10], x + 135, y + 22);
                        screen.DrawImage(digits[2], x + 155, y + 0);
                        screen.DrawImage(digits[9], x + 225, y + 0);
                        break;
                    case 30:
                        screen.DrawImage(digits[10], x + 135, y + 22);
                        screen.DrawImage(digits[3], x + 155, y + 0);
                        screen.DrawImage(digits[0], x + 225, y + 0);
                        break;
                    case 31:
                        screen.DrawImage(digits[10], x + 135, y + 22);
                        screen.DrawImage(digits[3], x + 155, y + 0);
                        screen.DrawImage(digits[1], x + 225, y + 0);
                        break;
                    case 32:
                        screen.DrawImage(digits[10], x + 135, y + 22);
                        screen.DrawImage(digits[3], x + 155, y + 0);
                        screen.DrawImage(digits[2], x + 225, y + 0);
                        break;
                    case 33:
                        screen.DrawImage(digits[10], x + 135, y + 22);
                        screen.DrawImage(digits[3], x + 155, y + 0);
                        screen.DrawImage(digits[3], x + 225, y + 0);
                        break;
                    case 34:
                        screen.DrawImage(digits[10], x + 135, y + 22);
                        screen.DrawImage(digits[3], x + 155, y + 0);
                        screen.DrawImage(digits[4], x + 225, y + 0);
                        break;
                    case 35:
                        screen.DrawImage(digits[10], x + 135, y + 22);
                        screen.DrawImage(digits[3], x + 155, y + 0);
                        screen.DrawImage(digits[5], x + 225, y + 0);
                        break;
                    case 36:
                        screen.DrawImage(digits[10], x + 135, y + 22);
                        screen.DrawImage(digits[3], x + 155, y + 0);
                        screen.DrawImage(digits[6], x + 225, y + 0);
                        break;
                    case 37:
                        screen.DrawImage(digits[10], x + 135, y + 22);
                        screen.DrawImage(digits[3], x + 155, y + 0);
                        screen.DrawImage(digits[7], x + 225, y + 0);
                        break;
                    case 38:
                        screen.DrawImage(digits[10], x + 135, y + 22);
                        screen.DrawImage(digits[3], x + 155, y + 0);
                        screen.DrawImage(digits[8], x + 225, y + 0);
                        break;
                    case 39:
                        screen.DrawImage(digits[10], x + 135, y + 22);
                        screen.DrawImage(digits[3], x + 155, y + 0);
                        screen.DrawImage(digits[9], x + 225, y + 0);
                        break;
                    case 40:
                        screen.DrawImage(digits[10], x + 135, y + 22);
                        screen.DrawImage(digits[4], x + 155, y + 0);
                        screen.DrawImage(digits[0], x + 225, y + 0);
                        break;
                    case 41:
                        screen.DrawImage(digits[10], x + 135, y + 22);
                        screen.DrawImage(digits[4], x + 155, y + 0);
                        screen.DrawImage(digits[1], x + 225, y + 0);
                        break;
                    case 42:
                        screen.DrawImage(digits[10], x + 135, y + 22);
                        screen.DrawImage(digits[4], x + 155, y + 0);
                        screen.DrawImage(digits[2], x + 225, y + 0);
                        break;
                    case 43:
                        screen.DrawImage(digits[10], x + 135, y + 22);
                        screen.DrawImage(digits[4], x + 155, y + 0);
                        screen.DrawImage(digits[3], x + 225, y + 0);
                        break;
                    case 44:
                        screen.DrawImage(digits[10], x + 135, y + 22);
                        screen.DrawImage(digits[4], x + 155, y + 0);
                        screen.DrawImage(digits[4], x + 225, y + 0);
                        break;
                    case 45:
                        screen.DrawImage(digits[10], x + 135, y + 22);
                        screen.DrawImage(digits[4], x + 155, y + 0);
                        screen.DrawImage(digits[5], x + 225, y + 0);
                        break;
                    case 46:
                        screen.DrawImage(digits[10], x + 135, y + 22);
                        screen.DrawImage(digits[4], x + 155, y + 0);
                        screen.DrawImage(digits[6], x + 225, y + 0);
                        break;
                    case 47:
                        screen.DrawImage(digits[10], x + 135, y + 22);
                        screen.DrawImage(digits[4], x + 155, y + 0);
                        screen.DrawImage(digits[7], x + 225, y + 0);
                        break;
                    case 48:
                        screen.DrawImage(digits[10], x + 135, y + 22);
                        screen.DrawImage(digits[4], x + 155, y + 0);
                        screen.DrawImage(digits[8], x + 225, y + 0);
                        break;
                    case 49:
                        screen.DrawImage(digits[10], x + 135, y + 22);
                        screen.DrawImage(digits[4], x + 155, y + 0);
                        screen.DrawImage(digits[9], x + 225, y + 0);
                        break;
                    case 50:
                        screen.DrawImage(digits[10], x + 135, y + 22);
                        screen.DrawImage(digits[5], x + 155, y + 0);
                        screen.DrawImage(digits[0], x + 225, y + 0);
                        break;
                    case 51:
                        screen.DrawImage(digits[10], x + 135, y + 22);
                        screen.DrawImage(digits[5], x + 155, y + 0);
                        screen.DrawImage(digits[1], x + 225, y + 0);
                        break;
                    case 52:
                        screen.DrawImage(digits[10], x + 135, y + 22);
                        screen.DrawImage(digits[5], x + 155, y + 0);
                        screen.DrawImage(digits[2], x + 225, y + 0);
                        break;
                    case 53:
                        screen.DrawImage(digits[10], x + 135, y + 22);
                        screen.DrawImage(digits[5], x + 155, y + 0);
                        screen.DrawImage(digits[3], x + 225, y + 0);
                        break;
                    case 54:
                        screen.DrawImage(digits[10], x + 135, y + 22);
                        screen.DrawImage(digits[5], x + 155, y + 0);
                        screen.DrawImage(digits[4], x + 225, y + 0);
                        break;
                    case 55:
                        screen.DrawImage(digits[10], x + 135, y + 22);
                        screen.DrawImage(digits[5], x + 155, y + 0);
                        screen.DrawImage(digits[5], x + 225, y + 0);
                        break;
                    case 56:
                        screen.DrawImage(digits[10], x + 135, y + 22);
                        screen.DrawImage(digits[5], x + 155, y + 0);
                        screen.DrawImage(digits[6], x + 225, y + 0);
                        break;
                    case 57:
                        screen.DrawImage(digits[10], x + 135, y + 22);
                        screen.DrawImage(digits[5], x + 155, y + 0);
                        screen.DrawImage(digits[7], x + 225, y + 0);
                        break;
                    case 58:
                        screen.DrawImage(digits[10], x + 135, y + 22);
                        screen.DrawImage(digits[5], x + 155, y + 0);
                        screen.DrawImage(digits[8], x + 225, y + 0);
                        break;
                    case 59:
                        screen.DrawImage(digits[10], x + 135, y + 22);
                        screen.DrawImage(digits[5], x + 155, y + 0);
                        screen.DrawImage(digits[9], x + 225, y + 0);
                        break;
                    default:
                        break;
                }

                switch (sec)
                {
                    case 0:
                        screen.DrawImage(digits[10], x + 290, y + 22);
                        screen.DrawImage(digits[0], x + 310, y + 0);
                        screen.DrawImage(digits[0], x + 380, y + 0);
                        break;
                    case 1:
                        screen.DrawImage(digits[10], x + 290, y + 22);
                        screen.DrawImage(digits[0], x + 310, y + 0);
                        screen.DrawImage(digits[1], x + 380, y + 0);
                        break;
                    case 2:
                        screen.DrawImage(digits[10], x + 290, y + 22);
                        screen.DrawImage(digits[0], x + 310, y + 0);
                        screen.DrawImage(digits[2], x + 380, y + 0);
                        break;
                    case 3:
                        screen.DrawImage(digits[10], x + 290, y + 22);
                        screen.DrawImage(digits[0], x + 310, y + 0);
                        screen.DrawImage(digits[3], x + 380, y + 0);
                        break;
                    case 4:
                        screen.DrawImage(digits[10], x + 290, y + 22);
                        screen.DrawImage(digits[0], x + 310, y + 0);
                        screen.DrawImage(digits[4], x + 380, y + 0);
                        break;
                    case 5:
                        screen.DrawImage(digits[10], x + 290, y + 22);
                        screen.DrawImage(digits[0], x + 310, y + 0);
                        screen.DrawImage(digits[5], x + 380, y + 0);
                        break;
                    case 6:
                        screen.DrawImage(digits[10], x + 290, y + 22);
                        screen.DrawImage(digits[0], x + 310, y + 0);
                        screen.DrawImage(digits[6], x + 380, y + 0);
                        break;
                    case 7:
                        screen.DrawImage(digits[10], x + 290, y + 22);
                        screen.DrawImage(digits[0], x + 310, y + 0);
                        screen.DrawImage(digits[7], x + 380, y + 0);
                        break;
                    case 8:
                        screen.DrawImage(digits[10], x + 290, y + 22);
                        screen.DrawImage(digits[0], x + 310, y + 0);
                        screen.DrawImage(digits[8], x + 380, y + 0);
                        break;
                    case 9:
                        screen.DrawImage(digits[10], x + 290, y + 22);
                        screen.DrawImage(digits[0], x + 310, y + 0);
                        screen.DrawImage(digits[9], x + 380, y + 0);
                        break;
                    case 10:
                        screen.DrawImage(digits[10], x + 290, y + 22);
                        screen.DrawImage(digits[1], x + 310, y + 0);
                        screen.DrawImage(digits[0], x + 380, y + 0);
                        break;
                    case 11:
                        screen.DrawImage(digits[10], x + 290, y + 22);
                        screen.DrawImage(digits[1], x + 310, y + 0);
                        screen.DrawImage(digits[1], x + 380, y + 0);
                        break;
                    case 12:
                        screen.DrawImage(digits[10], x + 290, y + 22);
                        screen.DrawImage(digits[1], x + 310, y + 0);
                        screen.DrawImage(digits[2], x + 380, y + 0);
                        break;
                    case 13:
                        screen.DrawImage(digits[10], x + 290, y + 22);
                        screen.DrawImage(digits[1], x + 310, y + 0);
                        screen.DrawImage(digits[3], x + 380, y + 0);
                        break;
                    case 14:
                        screen.DrawImage(digits[10], x + 290, y + 22);
                        screen.DrawImage(digits[1], x + 310, y + 0);
                        screen.DrawImage(digits[4], x + 380, y + 0);
                        break;
                    case 15:
                        screen.DrawImage(digits[10], x + 290, y + 22);
                        screen.DrawImage(digits[1], x + 310, y + 0);
                        screen.DrawImage(digits[5], x + 380, y + 0);
                        break;
                    case 16:
                        screen.DrawImage(digits[10], x + 290, y + 22);
                        screen.DrawImage(digits[1], x + 310, y + 0);
                        screen.DrawImage(digits[6], x + 380, y + 0);
                        break;
                    case 17:
                        screen.DrawImage(digits[10], x + 290, y + 22);
                        screen.DrawImage(digits[1], x + 310, y + 0);
                        screen.DrawImage(digits[7], x + 380, y + 0);
                        break;
                    case 18:
                        screen.DrawImage(digits[10], x + 290, y + 22);
                        screen.DrawImage(digits[1], x + 310, y + 0);
                        screen.DrawImage(digits[8], x + 380, y + 0);
                        break;
                    case 19:
                        screen.DrawImage(digits[10], x + 290, y + 22);
                        screen.DrawImage(digits[1], x + 310, y + 0);
                        screen.DrawImage(digits[9], x + 380, y + 0);
                        break;
                    case 20:
                        screen.DrawImage(digits[10], x + 290, y + 22);
                        screen.DrawImage(digits[2], x + 310, y + 0);
                        screen.DrawImage(digits[0], x + 380, y + 0);
                        break;
                    case 21:
                        screen.DrawImage(digits[10], x + 290, y + 22);
                        screen.DrawImage(digits[2], x + 310, y + 0);
                        screen.DrawImage(digits[1], x + 380, y + 0);
                        break;
                    case 22:
                        screen.DrawImage(digits[10], x + 290, y + 22);
                        screen.DrawImage(digits[2], x + 310, y + 0);
                        screen.DrawImage(digits[2], x + 380, y + 0);
                        break;
                    case 23:
                        screen.DrawImage(digits[10], x + 290, y + 22);
                        screen.DrawImage(digits[2], x + 310, y + 0);
                        screen.DrawImage(digits[3], x + 380, y + 0);
                        break;
                    case 24:
                        screen.DrawImage(digits[10], x + 290, y + 22);
                        screen.DrawImage(digits[2], x + 310, y + 0);
                        screen.DrawImage(digits[4], x + 380, y + 0);
                        break;
                    case 25:
                        screen.DrawImage(digits[10], x + 290, y + 22);
                        screen.DrawImage(digits[2], x + 310, y + 0);
                        screen.DrawImage(digits[5], x + 380, y + 0);
                        break;
                    case 26:
                        screen.DrawImage(digits[10], x + 290, y + 22);
                        screen.DrawImage(digits[2], x + 310, y + 0);
                        screen.DrawImage(digits[6], x + 380, y + 0);
                        break;
                    case 27:
                        screen.DrawImage(digits[10], x + 290, y + 22);
                        screen.DrawImage(digits[2], x + 310, y + 0);
                        screen.DrawImage(digits[7], x + 380, y + 0);
                        break;
                    case 28:
                        screen.DrawImage(digits[10], x + 290, y + 22);
                        screen.DrawImage(digits[2], x + 310, y + 0);
                        screen.DrawImage(digits[8], x + 380, y + 0);
                        break;
                    case 29:
                        screen.DrawImage(digits[10], x + 290, y + 22);
                        screen.DrawImage(digits[2], x + 310, y + 0);
                        screen.DrawImage(digits[9], x + 380, y + 0);
                        break;
                    case 30:
                        screen.DrawImage(digits[10], x + 290, y + 22);
                        screen.DrawImage(digits[3], x + 310, y + 0);
                        screen.DrawImage(digits[0], x + 380, y + 0);
                        break;
                    case 31:
                        screen.DrawImage(digits[10], x + 290, y + 22);
                        screen.DrawImage(digits[3], x + 310, y + 0);
                        screen.DrawImage(digits[1], x + 380, y + 0);
                        break;
                    case 32:
                        screen.DrawImage(digits[10], x + 290, y + 22);
                        screen.DrawImage(digits[3], x + 310, y + 0);
                        screen.DrawImage(digits[2], x + 380, y + 0);
                        break;
                    case 33:
                        screen.DrawImage(digits[10], x + 290, y + 22);
                        screen.DrawImage(digits[3], x + 310, y + 0);
                        screen.DrawImage(digits[3], x + 380, y + 0);
                        break;
                    case 34:
                        screen.DrawImage(digits[10], x + 290, y + 22);
                        screen.DrawImage(digits[3], x + 310, y + 0);
                        screen.DrawImage(digits[4], x + 380, y + 0);
                        break;
                    case 35:
                        screen.DrawImage(digits[10], x + 290, y + 22);
                        screen.DrawImage(digits[3], x + 310, y + 0);
                        screen.DrawImage(digits[5], x + 380, y + 0);
                        break;
                    case 36:
                        screen.DrawImage(digits[10], x + 290, y + 22);
                        screen.DrawImage(digits[3], x + 310, y + 0);
                        screen.DrawImage(digits[6], x + 380, y + 0);
                        break;
                    case 37:
                        screen.DrawImage(digits[10], x + 290, y + 22);
                        screen.DrawImage(digits[3], x + 310, y + 0);
                        screen.DrawImage(digits[7], x + 380, y + 0);
                        break;
                    case 38:
                        screen.DrawImage(digits[10], x + 290, y + 22);
                        screen.DrawImage(digits[3], x + 310, y + 0);
                        screen.DrawImage(digits[8], x + 380, y + 0);
                        break;
                    case 39:
                        screen.DrawImage(digits[10], x + 290, y + 22);
                        screen.DrawImage(digits[3], x + 310, y + 0);
                        screen.DrawImage(digits[9], x + 380, y + 0);
                        break;
                    case 40:
                        screen.DrawImage(digits[10], x + 290, y + 22);
                        screen.DrawImage(digits[4], x + 310, y + 0);
                        screen.DrawImage(digits[0], x + 380, y + 0);
                        break;
                    case 41:
                        screen.DrawImage(digits[10], x + 290, y + 22);
                        screen.DrawImage(digits[4], x + 310, y + 0);
                        screen.DrawImage(digits[1], x + 380, y + 0);
                        break;
                    case 42:
                        screen.DrawImage(digits[10], x + 290, y + 22);
                        screen.DrawImage(digits[4], x + 310, y + 0);
                        screen.DrawImage(digits[2], x + 380, y + 0);
                        break;
                    case 43:
                        screen.DrawImage(digits[10], x + 290, y + 22);
                        screen.DrawImage(digits[4], x + 310, y + 0);
                        screen.DrawImage(digits[3], x + 380, y + 0);
                        break;
                    case 44:
                        screen.DrawImage(digits[10], x + 290, y + 22);
                        screen.DrawImage(digits[4], x + 310, y + 0);
                        screen.DrawImage(digits[4], x + 380, y + 0);
                        break;
                    case 45:
                        screen.DrawImage(digits[10], x + 290, y + 22);
                        screen.DrawImage(digits[4], x + 310, y + 0);
                        screen.DrawImage(digits[5], x + 380, y + 0);
                        break;
                    case 46:
                        screen.DrawImage(digits[10], x + 290, y + 22);
                        screen.DrawImage(digits[4], x + 310, y + 0);
                        screen.DrawImage(digits[6], x + 380, y + 0);
                        break;
                    case 47:
                        screen.DrawImage(digits[10], x + 290, y + 22);
                        screen.DrawImage(digits[4], x + 310, y + 0);
                        screen.DrawImage(digits[7], x + 380, y + 0);
                        break;
                    case 48:
                        screen.DrawImage(digits[10], x + 290, y + 22);
                        screen.DrawImage(digits[4], x + 310, y + 0);
                        screen.DrawImage(digits[8], x + 380, y + 0);
                        break;
                    case 49:
                        screen.DrawImage(digits[10], x + 290, y + 22);
                        screen.DrawImage(digits[4], x + 310, y + 0);
                        screen.DrawImage(digits[9], x + 380, y + 0);
                        break;
                    case 50:
                        screen.DrawImage(digits[10], x + 290, y + 22);
                        screen.DrawImage(digits[5], x + 310, y + 0);
                        screen.DrawImage(digits[0], x + 380, y + 0);
                        break;
                    case 51:
                        screen.DrawImage(digits[10], x + 290, y + 22);
                        screen.DrawImage(digits[5], x + 310, y + 0);
                        screen.DrawImage(digits[1], x + 380, y + 0);
                        break;
                    case 52:
                        screen.DrawImage(digits[10], x + 290, y + 22);
                        screen.DrawImage(digits[5], x + 310, y + 0);
                        screen.DrawImage(digits[2], x + 380, y + 0);
                        break;
                    case 53:
                        screen.DrawImage(digits[10], x + 290, y + 22);
                        screen.DrawImage(digits[5], x + 310, y + 0);
                        screen.DrawImage(digits[3], x + 380, y + 0);
                        break;
                    case 54:
                        screen.DrawImage(digits[10], x + 290, y + 22);
                        screen.DrawImage(digits[5], x + 310, y + 0);
                        screen.DrawImage(digits[4], x + 380, y + 0);
                        break;
                    case 55:
                        screen.DrawImage(digits[10], x + 290, y + 22);
                        screen.DrawImage(digits[5], x + 310, y + 0);
                        screen.DrawImage(digits[5], x + 380, y + 0);
                        break;
                    case 56:
                        screen.DrawImage(digits[10], x + 290, y + 22);
                        screen.DrawImage(digits[5], x + 310, y + 0);
                        screen.DrawImage(digits[6], x + 380, y + 0);
                        break;
                    case 57:
                        screen.DrawImage(digits[10], x + 290, y + 22);
                        screen.DrawImage(digits[5], x + 310, y + 0);
                        screen.DrawImage(digits[7], x + 380, y + 0);
                        break;
                    case 58:
                        screen.DrawImage(digits[10], x + 290, y + 22);
                        screen.DrawImage(digits[5], x + 310, y + 0);
                        screen.DrawImage(digits[8], x + 380, y + 0);
                        break;
                    case 59:
                        screen.DrawImage(digits[10], x + 290, y + 22);
                        screen.DrawImage(digits[5], x + 310, y + 0);
                        screen.DrawImage(digits[9], x + 380, y + 0);
                        break;
                    default:
                        screen.DrawImage(digits[10], x + 290, y + 22);
                        screen.DrawImage(digits[0], x + 310, y + 0);
                        screen.DrawImage(digits[0], x + 380, y + 0);
                        break;
                }
                screen.Flush();
                Thread.Sleep(990);
            }
            Thread.Sleep(Timeout.Infinite);
        }
        private static void Sound_Alarm()
        {
            const int NOTE_C = 261;
            const int NOTE_D = 294;
            const int NOTE_E = 330;
            const int NOTE_F = 349;
            const int NOTE_G = 392;

            const int WHOLE_DURATION = 2000;
            const int EIGHTH = WHOLE_DURATION / 8;
            const int QUARTER = WHOLE_DURATION / 4;
            const int QUARTERDOT = WHOLE_DURATION / 3;
            const int HALF = WHOLE_DURATION / 2;
            const int WHOLE = WHOLE_DURATION;

            int[] note =  { NOTE_E, NOTE_E, NOTE_F, NOTE_G, NOTE_G, NOTE_F, NOTE_E,
                          NOTE_D, NOTE_C, NOTE_C, NOTE_D, NOTE_E, NOTE_E, NOTE_D,
                          NOTE_D, NOTE_E, NOTE_E, NOTE_F, NOTE_G, NOTE_G, NOTE_F,
                          NOTE_E, NOTE_D, NOTE_C, NOTE_C, NOTE_D, NOTE_E, NOTE_D,
                          NOTE_C, NOTE_C};

            int[] duration = { QUARTER, QUARTER, QUARTER, QUARTER, QUARTER, QUARTER,
                              QUARTER, QUARTER, QUARTER, QUARTER, QUARTER, QUARTER,
                              QUARTERDOT, EIGHTH, HALF, QUARTER, QUARTER, QUARTER, QUARTER,
                              QUARTER, QUARTER, QUARTER, QUARTER, QUARTER, QUARTER,
                              QUARTER, QUARTER, QUARTERDOT, EIGHTH, WHOLE};

            var controller = PwmController.FromName(SC20260.Timer.Pwm.Controller3.Id);
            var toneOut = controller.OpenChannel(SC20260.Timer.Pwm.Controller3.PB1);
            toneOut.SetActiveDutyCyclePercentage(0.5);

            while (true)
            {
                toneOut.Start();

                for (int i = 0; i < note.Length; i++)
                {
                    controller.SetDesiredFrequency(note[i]);
                    Thread.Sleep(duration[i]);
                }

                toneOut.Stop();
                Thread.Sleep(1000);
            }

        }
        private static void SetDisplayColor()
        {


        }

        private static void Touch_TouchUp(FT5xx6Controller sender, TouchEventArgs e)
        {

            Debug.WriteLine("Touch Up");

            Debug.WriteLine(e.X.ToString());
            Debug.WriteLine(e.Y.ToString());

            if (e.X == 0 && e.Y == 0)
            {
                color++;
                if (color >= 4)
                    color = 1;

            }

            if (e.X > 0 && e.X < 130 && e.Y < 85)
                hour = hour + 1;
            else if (e.X > 170 && e.X < 290 && e.Y < 85)
                min = min + 1;
            else if (e.X > 330 && e.X < 479 && e.Y < 85)
                sec = sec + 1;
            else if (e.X > 0 && e.X < 130 && e.Y > 200)
                hour = hour - 1;
            else if (e.X > 170 && e.X < 290 && e.Y < 250)
                min = min - 1;
            else if (e.X > 330 && e.X < 479 && e.Y < 250)
                sec = sec - 1;



            //if (color == 1) {

            //    for (x = 0; x < digits.Length; x++) {
            //        digits[x] = red[x];
            //    }

            //}
            //else if (color == 2) {

            //    for (x = 0; x < digits.Length; x++) {
            //        digits[x] = blue[x];
            //    }
            //}
            //else if (color == 3) {

            //    for (x = 0; x < digits.Length; x++) {
            //        digits[x] = green[x];
            //    }
            //}






        }

        private static void Touch_TouchDown(FT5xx6Controller sender, TouchEventArgs e)
        {
            Debug.WriteLine("Touch Down");

        }






    }
}