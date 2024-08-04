void DefaultFrag(Varyings input, out float4 outAlbedo : BUFFER_ALBEDO, out float4 outNormal: BUFFER_NORMAL)
{
    outAlbedo = _BaseColor;
    outNormal = float4(normalize(input.normalWS), 1.0);
}