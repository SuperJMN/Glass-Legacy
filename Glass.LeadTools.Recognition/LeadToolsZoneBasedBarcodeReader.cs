﻿namespace Glass.LeadTools.Recognition
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Windows.Media.Imaging;
    using Imaging;
    using Imaging.PostProcessing;
    using Imaging.ZoneConfigurations;
    using ImagingExtensions;
    using ImagingExtensions.ImageFilters;
    using Leadtools.Barcode;
    using Leadtools.Forms;

    public class LeadToolsZoneBasedBarcodeReader : IImageToTextConverter
    {
        public readonly IEnumerable<BarcodeStrategy> BarcodeStrategies = new[]
        {
            new BarcodeStrategy
            {
                ImageFilter = new NoProcessImageFilter(),
                ImageType = BarcodeImageType.ScannedDocument
            },
            new BarcodeStrategy
            {
                ImageFilter = new HistogramContrastImageFilter(),
                ImageType = BarcodeImageType.Picture
            },
            new BarcodeStrategy
            {
                ImageFilter = new HistogramContrastImageFilter(),
                ImageType = BarcodeImageType.ScannedDocument
            },
            new BarcodeStrategy
            {
                ImageFilter = new ExtendedFilter(),
                ImageType = BarcodeImageType.ScannedDocument
            },
            new BarcodeStrategy
            {
                ImageFilter = new AutoContrastImageFilter(),
                ImageType = BarcodeImageType.ScannedDocument
            }
        };

        public LeadToolsZoneBasedBarcodeReader(ILeadToolsLicenseApplier licenseApplier)
        {
            licenseApplier.ApplyLicense();
        }

        private BarcodeEngine BarcodeEngine { get; } = new BarcodeEngine
        {
            Reader = { ImageType = BarcodeImageType.Picture }
        };

        public BarcodeReadOptions[] CoreReadOptions { get; set; } = {
            new OneDBarcodeReadOptions
            {
                AllowPartialRead = false,
                SearchDirection = BarcodeSearchDirection.HorizontalAndVertical
            }
        };

        public IEnumerable<BarcodeSymbology> BarcodeSymbologies { get; set; } =
            new[]
            {
                BarcodeSymbology.Code3Of9,
                BarcodeSymbology.Code93,
                BarcodeSymbology.QR,
                BarcodeSymbology.Datamatrix
            };

        public IEnumerable<string> Recognize(BitmapSource bitmap, ZoneConfiguration barcodeConfig)
        {
            var coreReadOptions = CoreReadOptions;

            var leadRect = new LogicalRectangle(0, 0, bitmap.PixelWidth, bitmap.PixelHeight, LogicalUnit.Pixel);

            var recognitionUnits = from strategy in BarcodeStrategies.AsParallel()
                                   let filteredImage = Freeze(strategy.ImageFilter.Apply(bitmap))
                                   select new { ImageType = strategy.ImageType, FilteredImage = filteredImage };

            IEnumerable<string> identifiedTexts = new List<string>();
            foreach (var workUnit in recognitionUnits)
            {
                BarcodeEngine.Reader.ImageType = workUnit.ImageType;

                var barcodeDatas = BarcodeEngine.Reader.ReadBarcodes(
                    workUnit.FilteredImage.ToRasterImage(),
                    leadRect,
                    10,
                    BarcodeSymbologies.ToArray(),
                    coreReadOptions);

                var textForStrategy = barcodeDatas.Where(data => data.Value != null).Select(data => data.Value);
                identifiedTexts = identifiedTexts.Concat(textForStrategy);
            }

            return identifiedTexts;
        }

        private BitmapSource Freeze(BitmapSource apply)
        {
            apply.Freeze();
            return apply;
        }

        public IEnumerable<ImageTarget> ImageTargets => new Collection<ImageTarget> { new ImageTarget { Symbology = Symbology.Barcode, FilterTypes = FilterType.All } };
    }
}