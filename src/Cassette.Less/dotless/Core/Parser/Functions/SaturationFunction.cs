namespace dotless.Core.Parser.Functions
{
    using Infrastructure.Nodes;
    using Tree;
    using Utils;

    class SaturationFunction : HslColorFunctionBase
    {
        protected override Node EvalHsl(HslColor color)
        {
            return color.GetSaturation();
        }

        protected override Node EditHsl(HslColor color, Number number)
        {
            color.Saturation += number.Value/100;
            return color.ToRgbColor();
        }
    }

    class SaturateFunction : SaturationFunction {}

    class DesaturateFunction : SaturationFunction
    {
        protected override Node EditHsl(HslColor color, Number number)
        {
            return base.EditHsl(color, -number);
        }
    }
}