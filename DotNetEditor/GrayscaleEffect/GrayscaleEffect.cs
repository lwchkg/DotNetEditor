using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Reflection;

namespace GrayscaleEffect
{

    public class GrayscaleEffect : ShaderEffect
    {
        private static PixelShader _pixelShader =
            new PixelShader() { UriSource = MakePackUri("GrayscaleEffect/GrayscaleEffect.fx.ps") };

        public GrayscaleEffect()
        {
            PixelShader = _pixelShader;
            UpdateShaderValue(InputProperty);
        }

        // MakePackUri is a utility method for computing a pack uri
        // for the given resource. 
        public static Uri MakePackUri(string relativeFile)
        {
            Assembly a = typeof(GrayscaleEffect).Assembly;

            // Extract the short name.
            string assemblyShortName = a.ToString().Split(',')[0];

            string uriString = "pack://application:,,,/" +
                assemblyShortName +
                ";component/" +
                relativeFile;

            return new Uri(uriString);
        }

        ///////////////////////////////////////////////////////////////////////
        #region Input dependency property

        public Brush Input
        {
            get { return (Brush)GetValue(InputProperty); }
            set { SetValue(InputProperty, value); }
        }

        public static readonly DependencyProperty InputProperty =
            ShaderEffect.RegisterPixelShaderSamplerProperty("Input", typeof(GrayscaleEffect), 0);

        #endregion
    }
}
