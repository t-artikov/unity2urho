#ifdef COMPILEPS
float3 GetBakedLighting(float2 tc)
{
    float4 data = Sample2D(EmissiveMap, tc).rgba;
    return data.rgb * data.a * 4.0;
}
#endif