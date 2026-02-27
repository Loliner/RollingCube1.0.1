Shader "Custom/DepthOnly"
{
    SubShader
    {
        // 标记为透明队列，确保能被正确识别
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        Pass
        {
            // 核心：开启深度写入
            ZWrite On
            // 核心：颜色遮罩设为0（也就是不输出任何颜色，相当于 R,G,B,A 都没勾）
            ColorMask 0
        }
    }
}