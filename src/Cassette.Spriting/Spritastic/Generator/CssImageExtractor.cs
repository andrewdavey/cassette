using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Cassette.Spriting.Spritastic.Parser;
using Cassette.Spriting.Spritastic.Selector;

namespace Cassette.Spriting.Spritastic.Generator
{
    class CssImageExtractor : ICssImageExtractor
    {
        private readonly ICssSelectorAnalyzer cssSelectorAnalyzer;
        private readonly IComparer<BackgroundImageClass> selectorComparer = new SelectorComparer();
        private readonly Regex classPattern = new Regex(@"(?<=\}|)[^\}]+\}", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline);

        public CssImageExtractor(ICssSelectorAnalyzer cssSelectorAnalyzer)
        {
            this.cssSelectorAnalyzer = cssSelectorAnalyzer;
        }

        public IEnumerable<BackgroundImageClass> ExtractImageUrls(string cssContent)
        {
            var finalUrls = new List<BackgroundImageClass>();
            var draftUrls = new List<BackgroundImageClass>();
            var classCounter = 0;
            foreach (var classMatch in classPattern.Matches(cssContent))
            {
                var imageClass = new BackgroundImageClass(classMatch.ToString(), ++classCounter);
                if (!ShouldFlatten(imageClass)) continue;
                if (!IsComplete(imageClass) && ShouldFlatten(imageClass))
                {
                    var workList = new List<BackgroundImageClass>();
                    for (var n = draftUrls.Count - 1; n > -1; n--)
                    {
                        var selectors = draftUrls[n].Selector.Split(new [] {','});
                        var targetSelectors = imageClass.Selector.Split(new[] { ',' });
                        foreach (var selector in selectors)
                        {
                            if (targetSelectors
                                .Any(targetSelector => cssSelectorAnalyzer.IsInScopeOfTarget(targetSelector.Trim(), selector.Trim())))
                                workList.Add(draftUrls[n]);
                        }
                    }
                    workList.Sort(selectorComparer);
                    foreach (var cls in workList.Where(cls => !IsComplete(imageClass)))
                        InheritMissingProperties(cls, imageClass);
                }

                draftUrls.Add(imageClass);
                if (CanSprite(imageClass))
                    finalUrls.Add(imageClass);
            }
            return finalUrls;
        }

        private static bool DerivableHasMissingProperty(BackgroundImageClass derrivableClass, BackgroundImageClass imageClass, PropertyCompletion property)
        {
            if ((imageClass.PropertyCompletion & property) != property && (derrivableClass.PropertyCompletion & property) == property)
            {
                imageClass.PropertyCompletion = imageClass.PropertyCompletion | property;
                return true;
            }
            return false;
        }

        private static void InheritMissingProperties(BackgroundImageClass derrivableClass, BackgroundImageClass imageClass)
        {
            if (DerivableHasMissingProperty(derrivableClass, imageClass, PropertyCompletion.HasYOffset))
                imageClass.YOffset = derrivableClass.YOffset;

            if (DerivableHasMissingProperty(derrivableClass, imageClass, PropertyCompletion.HasXOffset))
                imageClass.XOffset = derrivableClass.XOffset;

            if (DerivableHasMissingProperty(derrivableClass, imageClass, PropertyCompletion.HasRepeat))
                imageClass.Repeat = derrivableClass.Repeat;

            if (DerivableHasMissingProperty(derrivableClass, imageClass, PropertyCompletion.HasImage))
                imageClass.ImageUrl = derrivableClass.ImageUrl;

            if (DerivableHasMissingProperty(derrivableClass, imageClass, PropertyCompletion.HasPaddingBottom))
                imageClass.PaddingBottom = derrivableClass.PaddingBottom;

            if (DerivableHasMissingProperty(derrivableClass, imageClass, PropertyCompletion.HasPaddingTop))
                imageClass.PaddingTop = derrivableClass.PaddingTop;

            if (DerivableHasMissingProperty(derrivableClass, imageClass, PropertyCompletion.HasPaddingLeft))
                imageClass.PaddingLeft = derrivableClass.PaddingLeft;

            if (DerivableHasMissingProperty(derrivableClass, imageClass, PropertyCompletion.HasPaddingRight))
                imageClass.PaddingRight = derrivableClass.PaddingRight;

            if (DerivableHasMissingProperty(derrivableClass, imageClass, PropertyCompletion.HasWidth))
                imageClass.ExplicitWidth = derrivableClass.ExplicitWidth;

            if (DerivableHasMissingProperty(derrivableClass, imageClass, PropertyCompletion.HasHeight))
                imageClass.ExplicitHeight = derrivableClass.ExplicitHeight;
        }

        private bool CanSprite(BackgroundImageClass imageClass)
        {
            if (imageClass.ImageUrl == null)
                return false;

            if (imageClass.Width > 0
                && imageClass.Repeat == RepeatStyle.NoRepeat
                && ((imageClass.YOffset.PositionMode == PositionMode.Direction)
                    || (imageClass.YOffset.PositionMode == PositionMode.Percent)
                    || ((imageClass.YOffset.PositionMode == PositionMode.Unit) && (imageClass.YOffset.Offset >= 0 || imageClass.Height > 0))))
                return true;
            return false;
        }

        private bool IsComplete(BackgroundImageClass imageClass)
        {
            return (imageClass.PropertyCompletion &
                   (PropertyCompletion.HasHeight | PropertyCompletion.HasImage | PropertyCompletion.HasPaddingBottom |
                    PropertyCompletion.HasPaddingLeft | PropertyCompletion.HasPaddingRight |
                    PropertyCompletion.HasPaddingTop | PropertyCompletion.HasRepeat | PropertyCompletion.HasWidth |
                    PropertyCompletion.HasXOffset | PropertyCompletion.HasYOffset)) ==
                   (PropertyCompletion.HasHeight | PropertyCompletion.HasImage | PropertyCompletion.HasPaddingBottom |
                    PropertyCompletion.HasPaddingLeft | PropertyCompletion.HasPaddingRight |
                    PropertyCompletion.HasPaddingTop | PropertyCompletion.HasRepeat | PropertyCompletion.HasWidth |
                    PropertyCompletion.HasXOffset | PropertyCompletion.HasYOffset);
        }

        private bool ShouldFlatten(BackgroundImageClass imageClass)
        {
            if ((imageClass.PropertyCompletion & PropertyCompletion.HasImage) == PropertyCompletion.HasImage)
                return true;
            if ((imageClass.PropertyCompletion & PropertyCompletion.HasXOffset) == PropertyCompletion.HasXOffset)
                return true;
            if ((imageClass.PropertyCompletion & PropertyCompletion.HasYOffset) == PropertyCompletion.HasYOffset)
                return true;

            return false;
        }
    }
}