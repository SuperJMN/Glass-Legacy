﻿namespace Glass.Barcodes.MessagingToolkit
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Windows.Media.Imaging;
    using global::MessagingToolkit.Barcode;
    using Imaging;
    using Imaging.PostProcessing;
    using Imaging.ZoneConfigurations;

    public class MessagingToolkitZoneBasedBarcodeReader : IImageToTextConverter
    {
        private readonly BarcodeDecoder barcodeReader = new BarcodeDecoder();

        public IEnumerable<RecognitionResult> Recognize(BitmapSource bitmap, ZoneConfiguration config)
        {
            var writeableBitmap = new WriteableBitmap(bitmap);
            string text;
            try
            {
                var result = barcodeReader.Decode(writeableBitmap, new Dictionary<DecodeOptions, object>());
                text = result.Text;
            }
            catch
            {
                yield break;
            }

            yield return new RecognitionResult(text, 1D);
        }

        public IEnumerable<ImageTarget> ImageTargets => new Collection<ImageTarget> { new ImageTarget { Symbology = Symbology.Barcode, FilterTypes = FilterType.All } };
    }
}