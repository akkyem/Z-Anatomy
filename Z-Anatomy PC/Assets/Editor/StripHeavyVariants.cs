using System.Collections.Generic;
using UnityEditor.Build;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.Rendering;

class StripHeavyVariants : IPreprocessShaders
{
    public int callbackOrder => 0;

    static readonly string[] stripKeywords = {
        "LIGHTMAP_ON", "DIRLIGHTMAP_COMBINED", "DYNAMICLIGHTMAP_ON",
        "LIGHTMAP_SHADOW_MIXING", "SHADOWS_SHADOWMASK", "_MIXED_LIGHTING_SUBTRACTIVE",
        "_ADDITIONAL_LIGHT_SHADOWS", "_FORWARD_PLUS", "_CLUSTER_LIGHT_LOOP",
        "_SCREEN_SPACE_OCCLUSION", "_LIGHT_COOKIES", "_LIGHT_LAYERS",
        "_REFLECTION_PROBE_BLENDING", "_REFLECTION_PROBE_BOX_PROJECTION",
        "_SHADOWS_SOFT", "_SHADOWS_SOFT_LOW", "_SHADOWS_SOFT_MEDIUM", "_SHADOWS_SOFT_HIGH",
        "DEBUG_DISPLAY", "SCREEN_COORD_OVERRIDE",
        "_FILM_GRAIN", "_CHROMATIC_ABERRATION", "_DISTORTION",
        "_BLOOM_HQ_DIRT", "_BLOOM_LQ_DIRT"
    };

    static bool IsOn(ShaderCompilerData d, Shader s, string kw)
    {
        var set = d.shaderKeywordSet;
        if (set.IsEnabled(new ShaderKeyword(kw))) return true;
        var local = new ShaderKeyword(s, kw);
        return local.IsValid() && set.IsEnabled(local);
    }

    public void OnProcessShader(Shader shader, ShaderSnippetData snippet,
                                IList<ShaderCompilerData> data)
    {
        string n = shader.name;

        if (n == "Universal Render Pipeline/Complex Lit") { data.Clear(); return; }

        if (!(n.StartsWith("Shader Graphs/") ||
              n.StartsWith("Universal Render Pipeline/") ||
              n.StartsWith("Hidden/Universal Render Pipeline/"))) return;

        if (snippet.passName == "GBuffer" || snippet.passName == "UberPostXR")
        { data.Clear(); return; }

        for (int i = data.Count - 1; i >= 0; i--)
            foreach (var kw in stripKeywords)
                if (IsOn(data[i], shader, kw)) { data.RemoveAt(i); break; }
    }
}
