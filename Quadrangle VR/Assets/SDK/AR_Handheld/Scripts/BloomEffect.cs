using UnityEngine;
using System.Collections;

// This class implements a bloom effect.
// It is based off of OTEE's BlurEffect.cs
// It uses texture combiners, so it should run on older gfx. cards too.
// It is based on a four tap cone kernel.
[AddComponentMenu("Image Effects/Bloom")]
public class BloomEffect : MonoBehaviour {

	/// 0 to 1... the amount of bloom effect
	public float bloomAmount = 1F;

	/// How many levels of blurryness we want
	public int blurLevels = 2;

	static Material m_BloomMaterial = null;
	protected static Material bloomMaterial {
		get {
			if (m_BloomMaterial == null) 
				m_BloomMaterial = new Material (
					"Shader \"Bloom Combine Shader\" {" +
					"   Properties { _Color (\"Bloom Amount\", Color) = (1,1,1,1) }" +
					"   SubShader { Pass {" +
					"       ZTest Always Cull Off ZWrite Off" +
					"       Blend SrcAlpha One" +
					"       SetTexture [__RenderTex] {" +
					"           constantColor [_Color]" +
					"           combine constant * texture QUAD" +
					"}}}}"
				);
			return m_BloomMaterial;
		} 
	}

	/// The blur shader material.
	static Material m_BlurMaterial = null;  
	protected static Material blurMaterial {
		get {
			if (m_BlurMaterial == null) 
				m_BlurMaterial = new Material (
					"Shader \"ConeTab\" {\n"    +
					"   SubShader { Pass {\n" +
					"       ZTest Always Cull Off ZWrite Off\n" +
					"       SetTexture [__RenderTex] { combine texture * constant alpha " +
					"           constantColor (0,0,0,0.25) } \n"    +
					"       SetTexture [__RenderTex] { combine texture * constant + previous " +
					"           constantColor (0,0,0,0.25) }\n" +
					"       SetTexture [__RenderTex] { combine texture * constant + previous " +
					"           constantColor (0,0,0,0.25) }\n" +
					"       SetTexture [__RenderTex] { combine texture * constant + previous " +
					"           constantColor (0,0,0,0.25) }\n" +
					"   }}\n"    +
					"}"
				);
			return m_BlurMaterial;
		} 
	}

	protected void Start()
	{
		// Disable the image effect if the shader can't
		// run on the users graphics card
		if (!blurMaterial.shader.isSupported)
			enabled = false;
	}

	// Function that actually renders the screenspace passes, offsetting the texture coordinates
	// based one the size of the screen.
	public static void FourTapCone (RenderTexture source, Rect sourceRect, RenderTexture dest, Rect destRect, int iteration) {

		RenderTexture.active = dest;
		source.SetGlobalShaderProperty ("__RenderTex");

		float offsetX = (.5F+iteration) / (float)source.width;
		float offsetY = (.5F+iteration) / (float)source.height;

		GL.PushMatrix ();
		GL.LoadOrtho ();    

		for (int i = 0; i < blurMaterial.passCount; i++) {
			blurMaterial.SetPass (i);

			GL.Begin (GL.QUADS);
			GL.MultiTexCoord2 (0, sourceRect.xMin - offsetX,    sourceRect.yMin - offsetY);
			GL.MultiTexCoord2 (1, sourceRect.xMin + offsetX,    sourceRect.yMin - offsetY);
			GL.MultiTexCoord2 (2, sourceRect.xMin + offsetX,    sourceRect.yMin + offsetY); 
			GL.MultiTexCoord2 (3, sourceRect.xMin - offsetX,    sourceRect.yMin + offsetY);
			GL.Vertex3 (destRect.xMin,destRect.yMin, .1f);

			GL.MultiTexCoord2 (0, sourceRect.xMax - offsetX,    sourceRect.yMin - offsetY);
			GL.MultiTexCoord2 (1, sourceRect.xMax + offsetX,    sourceRect.yMin - offsetY);
			GL.MultiTexCoord2 (2, sourceRect.xMax + offsetX,    sourceRect.yMin + offsetY); 
			GL.MultiTexCoord2 (3, sourceRect.xMax - offsetX,    sourceRect.yMin + offsetY);
			GL.Vertex3 (destRect.xMax,destRect.yMin, .1f);

			GL.MultiTexCoord2 (0, sourceRect.xMax - offsetX,    sourceRect.yMax - offsetY);
			GL.MultiTexCoord2 (1, sourceRect.xMax + offsetX,    sourceRect.yMax - offsetY);
			GL.MultiTexCoord2 (2, sourceRect.xMax + offsetX,    sourceRect.yMax + offsetY); 
			GL.MultiTexCoord2 (3, sourceRect.xMax - offsetX,    sourceRect.yMax + offsetY);
			GL.Vertex3 (destRect.xMax,destRect.yMax,.1f);

			GL.MultiTexCoord2 (0, sourceRect.xMin - offsetX,    sourceRect.yMax - offsetY);
			GL.MultiTexCoord2 (1, sourceRect.xMin + offsetX,    sourceRect.yMax - offsetY);
			GL.MultiTexCoord2 (2, sourceRect.xMin + offsetX,    sourceRect.yMax + offsetY); 
			GL.MultiTexCoord2 (3, sourceRect.xMin - offsetX,    sourceRect.yMax + offsetY);
			GL.Vertex3 (destRect.xMin,destRect.yMax,.1f);
			GL.End();

		}
		GL.PopMatrix ();

	}

	// Downsamples the texture to a quarter resolution.
	static void DownSample4x (RenderTexture source, Rect sourceRect, RenderTexture dest, Rect destRect) {

		//if (dest.width * (destRect.xMax - destRect.xMin) *4 != source.width * (sourceRect.xMax - sourceRect.xMin))
		//  Debug.Log ("Warning: DownSample4x called with non-matching rectangles");
		RenderTexture.active = dest;
		source.SetGlobalShaderProperty ("__RenderTex");

		float offsetX = 1F / (float)source.width;
		float offsetY = 1F / (float)source.height;

		GL.PushMatrix ();
		GL.LoadOrtho ();        
		for (int i = 0; i < blurMaterial.passCount; i++) {
			blurMaterial.SetPass (i);

			GL.Begin (GL.QUADS);
			GL.MultiTexCoord2 (0, sourceRect.xMin - offsetX,    sourceRect.yMin - offsetY);
			GL.MultiTexCoord2 (1, sourceRect.xMin + offsetX,    sourceRect.yMin - offsetY);
			GL.MultiTexCoord2 (2, sourceRect.xMin + offsetX,    sourceRect.yMin + offsetY); 
			GL.MultiTexCoord2 (3, sourceRect.xMin - offsetX,    sourceRect.yMin + offsetY);
			GL.Vertex3 (destRect.xMin,destRect.yMin, .1f);

			GL.MultiTexCoord2 (0, sourceRect.xMax - offsetX,    sourceRect.yMin - offsetY);
			GL.MultiTexCoord2 (1, sourceRect.xMax + offsetX,    sourceRect.yMin - offsetY);
			GL.MultiTexCoord2 (2, sourceRect.xMax + offsetX,    sourceRect.yMin + offsetY); 
			GL.MultiTexCoord2 (3, sourceRect.xMax - offsetX,    sourceRect.yMin + offsetY);
			GL.Vertex3 (destRect.xMax,destRect.yMin, .1f);

			GL.MultiTexCoord2 (0, sourceRect.xMax - offsetX,    sourceRect.yMax - offsetY);
			GL.MultiTexCoord2 (1, sourceRect.xMax + offsetX,    sourceRect.yMax - offsetY);
			GL.MultiTexCoord2 (2, sourceRect.xMax + offsetX,    sourceRect.yMax + offsetY); 
			GL.MultiTexCoord2 (3, sourceRect.xMax - offsetX,    sourceRect.yMax + offsetY);
			GL.Vertex3 (destRect.xMax,destRect.yMax,.1f);

			GL.MultiTexCoord2 (0, sourceRect.xMin - offsetX,    sourceRect.yMax - offsetY);
			GL.MultiTexCoord2 (1, sourceRect.xMin + offsetX,    sourceRect.yMax - offsetY);
			GL.MultiTexCoord2 (2, sourceRect.xMin + offsetX,    sourceRect.yMax + offsetY); 
			GL.MultiTexCoord2 (3, sourceRect.xMin - offsetX,    sourceRect.yMax + offsetY);
			GL.Vertex3 (destRect.xMin,destRect.yMax,.1f);
			GL.End();
		}
		GL.PopMatrix ();
	}

	// Update is called once per frame
	void OnRenderImage (RenderTexture source, RenderTexture destination) {      
		RenderTexture buffer = RenderTexture.GetTemporary(source.width/4, source.height/4, 0);
		RenderTexture buffer2 = RenderTexture.GetTemporary(source.width/4, source.height/4, 0);

		// Blur the source image. Since blurring is an expensive process, we want to do it at
		// quater resolution.
		// Copy the main screen image into an image at half size
		DownSample4x (source, new Rect (0,0,1,1), buffer, new Rect (0,0,1,1));  

		// Blur the image by running the FourTapCone function on it i = blurLevels numbers of time.
		bool oddEven = true;
		for(int i = 0; i < blurLevels; i++) {
			if(oddEven) {
				FourTapCone (buffer, new Rect (0,0,1,1), buffer2, new Rect (0,0,1,1), i);
				oddEven = false;
			} else {
				FourTapCone (buffer2, new Rect (0,0,1,1), buffer, new Rect (0,0,1,1), i);
				oddEven = true;
			}
		}
		Graphics.Blit(source,destination);

		//bloomAmount = Mathf.Clamp01(bloomAmount);
		bloomMaterial.color = new Color(1F, 1F, 1F, bloomAmount);

		if(oddEven)
			BlitBloom(buffer, destination);
		else
			BlitBloom(buffer2, destination);

		RenderTexture.ReleaseTemporary(buffer);
		RenderTexture.ReleaseTemporary(buffer2);
	}

	public void BlitBloom (RenderTexture source, RenderTexture dest)
	{
		// Make the destination texture the target for all rendering
		RenderTexture.active = dest;        
		// Assign the source texture to a property from a shader
		source.SetGlobalShaderProperty ("__RenderTex"); 
		// Set up the simple Matrix
		GL.PushMatrix ();
		GL.LoadOrtho ();
		bloomMaterial.SetPass (0);
		//ImageEffects.DrawGrid(1,1);
		GL.PopMatrix ();
	}

}