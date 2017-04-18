Shader "Multiply by Solid Color" {

	Properties
	{
		_Color("Color", Color) = (1,1,1)
	}

		SubShader
	{
		Tags{ Queue = Transparent }
		Blend Zero SrcColor
		Pass{ Color[_Color] }
	}

}
