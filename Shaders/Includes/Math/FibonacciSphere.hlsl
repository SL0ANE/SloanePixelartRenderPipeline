float3 FibonacciSphereMap(float3 inputVector, int numPoints) {
    float phi = 3.14159265359 * (3.0 - sqrt(5.0));
    float3 bestPoint = float3(0.0, 0.0, 0.0);
    float maxDot = -1.0;
    for (int i = 0; i < numPoints; ++i) {
        float y = 1.0 - float(i) / float(numPoints - 1) * 2.0;
        float r = sqrt(1.0 - y * y);
        float theta = float(i) * phi;

        float3 pointOnSphere = float3(cos(theta) * r, y, sin(theta) * r);
        float dotProduct = dot(normalize(inputVector), pointOnSphere);
        
        if (dotProduct > maxDot) {
            maxDot = dotProduct;
            bestPoint = pointOnSphere;
        }
    }
    return bestPoint;
}
