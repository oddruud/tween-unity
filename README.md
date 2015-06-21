# tween-unity
A lightweight tweening library for unity3D to tween floats, vectors, quaternions and colors. Easily combine easing functions for more complex effects. Tween either based on time or speed.

# usage:
Create a GameObject and attach the TweenController behaviour. 

Looping yoyo scale tween example:
```C#
Using Rucrede;

Vector 3 scale = someTransform.localScale;
Tween scaleTween = Tween.to(scale, scale * 2f, 1.0f, Tween.EaseType.easeInCubicOutSine, 
  (Tween tween) => {someTransform.localScale = (Vector3) tween.Value;},
  (Tween tween) => {/*handle complete*/}
  , 0f, true, -1);
```

Color tween example:
```C#
Using Rucrede;

Color color1 = new Color(0f, 0f,0f, 1.0f);
Color color2 = new Color(0f, 0f,0f, 1.0f);
Tween colorTween = Tween.to(color1, color2, 1.0f, Tween.EaseType.easeInBack, 
  (Tween tween) => {someMaterial.SetColor("_COLOR", (Color) tween.Value);},
  (Tween tween) => {/*handle complete*/},
  , 0f, false, 0);
```

Rotate tween effect example:
```C#
Using Rucrede;

Quaternion toRotation = someTransform.rotation * Quaternion.Euler(Vector3.up * 90f);
Tween rotateTween = Tween.to(someTransform.rotation, toRotation, 1.0f, Tween.EaseType.easeInBounce, 
  (Tween tween) => {someTransform.rotation = (Quaternion) tween.Value;},
  (Tween tween) => {/*handle complete*/},
  , 0f, false, 0);
```

