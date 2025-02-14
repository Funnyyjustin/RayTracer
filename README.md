# Whitted-style Ray Tracer

This repository contains a fairly simple Ray Tracer, written in C#, and made by myself. The main inspiration of the Ray Tracer came from [the Ray Tracing in One Weekend book](https://raytracing.github.io/books/RayTracingInOneWeekend.html), but as the book was written in C++,
some differences may occur when one compares the code in this repository with the coding examples in the book.

If you have any questions, feel free to reach out to me.

## Main features

- The ray tracer currently only supports spheres as primitives.
- The ray tracer supports multiple primitives in one scene.
- The ray tracer currently supports diffuse and dielectric materials, as well as metals.
	- Diffuse materials are represented by Lambertian reflectance.
- The ray tracer supports antialiasing.

## Future work

- Currently, the camera is only static in one location. I plan on adding support for the camera, so it can be placed and rotated.
- The camera will in the future also be able to change field of view, and I also plan to implement depth of field.
- I might look into moving the camera around during runtime, and generating a new image if the user wants to move around.
	- I have tackled this problem already in the past, when I was following a graphics course at Utrecht University. The performance was horrible, and I'd like to have at least relatively okay performance if I'm going to implement this feature.
- I plan on implementing normal mapping, so it would be possible to add custom textures to the primitives.
- I also plan to implement more primitives, such as triangles and rectangles, and perhaps even go furhter and work on meshes.
- I also plan on implementing more materials, although the most important ones have already been covered.
- Who knows what else the future might hold?
