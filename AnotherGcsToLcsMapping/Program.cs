using System;

public struct Point3D
{
    public double X, Y, Z;

    public Point3D(double x, double y, double z)
    {
        X = x;
        Y = y;
        Z = z;
    }
}

public class CoordinateTransformer
{
    /* Function to normalize a vector. This gets rid of magnitude but retains general direction. */
    public static Point3D Normalize(Point3D vector)
    {
        double magnitude = Math.Sqrt(vector.X * vector.X + vector.Y * vector.Y + vector.Z * vector.Z);
        if (magnitude == 0)
            throw new InvalidOperationException("Cannot normalize a zero vector.");

        return new Point3D(vector.X / magnitude, vector.Y / magnitude, vector.Z / magnitude);
    }

    /* Computes the cross product of two vectors */
    public static Point3D CrossProduct(Point3D a, Point3D b)
    {
        return new Point3D(
            a.Y * b.Z - a.Z * b.Y,
            a.Z * b.X - a.X * b.Z,
            a.X * b.Y - a.Y * b.X
        );
    }

    /* Computes the LCS rotation matrix where zDirection is presented by the MajorAxisVector */
    public static double[,] ComputeRotationMatrix(Point3D xDirection, Point3D zDirection)
    {
        Point3D x = Normalize(xDirection);
        Point3D z = Normalize(zDirection);
        Point3D y = Normalize(CrossProduct(z, x));

        y = new Point3D(y.X, y.Y, y.Z);

        Console.WriteLine($"X axis of lcs:\n({x.X},{x.Y},{x.Z})");
        Console.WriteLine($"Z axis of lcs:\n({z.X},{z.Y},{z.Z})");
        Console.WriteLine($"Y axis of lcs:\n({y.X},{y.Y},{y.Z})");

        return new double[,]
        {
            { x.X, y.X, z.X },
            { x.Y, y.Y, z.Y },
            { x.Z, y.Z, z.Z }
        };
    }

    public static Point3D LcsToGcs(Point3D pointLcs, Point3D lcsOrigin, double[,] rotationMatrix)
    {
        double x = rotationMatrix[0, 0] * pointLcs.X +
                   rotationMatrix[0, 1] * pointLcs.Y +
                   rotationMatrix[0, 2] * pointLcs.Z;

        double y = rotationMatrix[1, 0] * pointLcs.X +
                   rotationMatrix[1, 1] * pointLcs.Y +
                   rotationMatrix[1, 2] * pointLcs.Z;

        double z = rotationMatrix[2, 0] * pointLcs.X +
                   rotationMatrix[2, 1] * pointLcs.Y +
                   rotationMatrix[2, 2] * pointLcs.Z;

        return new Point3D(x + lcsOrigin.X, y + lcsOrigin.Y, z + lcsOrigin.Z);
    }

    public static Point3D GcsToLcs(Point3D pointGcs, Point3D lcsOrigin, double[,] rotationMatrix)
    {
        Point3D translatedPoint = new Point3D(
            pointGcs.X - lcsOrigin.X,
            pointGcs.Y - lcsOrigin.Y,
            pointGcs.Z - lcsOrigin.Z
        );

        /* Compute the transpose of the rotation matrix */
        double[,] rotationMatrixTranspose = new double[3, 3];
        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                rotationMatrixTranspose[i, j] = rotationMatrix[j, i];
            }
        }

        /* Apply the transpose rotation matrix */
        double x = rotationMatrixTranspose[0, 0] * translatedPoint.X +
                   rotationMatrixTranspose[0, 1] * translatedPoint.Y +
                   rotationMatrixTranspose[0, 2] * translatedPoint.Z;

        double y = rotationMatrixTranspose[1, 0] * translatedPoint.X +
                   rotationMatrixTranspose[1, 1] * translatedPoint.Y +
                   rotationMatrixTranspose[1, 2] * translatedPoint.Z;

        double z = rotationMatrixTranspose[2, 0] * translatedPoint.X +
                   rotationMatrixTranspose[2, 1] * translatedPoint.Y +
                   rotationMatrixTranspose[2, 2] * translatedPoint.Z;

        return new Point3D(x, y, z);
    }
}

class Program
{
    static void Main()
    {
        // Horizontal member example
        Point3D xDirectionHorizontal = new Point3D(0, 1, 0);
        Point3D zDirectionHorizontal = new Point3D(0, 0, 1);
        double[,] rotationMatrixHorizontal = CoordinateTransformer.ComputeRotationMatrix(xDirectionHorizontal, zDirectionHorizontal);

        Point3D lcsOriginHorizontal = new Point3D(500000, 300000, 100000);
        Point3D detailingVectorLcs = new Point3D(-100, -40, 50);

        Point3D pointGcsHorizontal = CoordinateTransformer.LcsToGcs(detailingVectorLcs, lcsOriginHorizontal, rotationMatrixHorizontal);
        Console.WriteLine($"Horizontal Detailed Position: E {pointGcsHorizontal.X}, N {pointGcsHorizontal.Y}, U {pointGcsHorizontal.Z}");
        Console.WriteLine($"Expected Horizontal Detailed Position: E {500_040}, N {299_900}, U {100_050}");
        Point3D recomputedHorizontal = CoordinateTransformer.GcsToLcs(pointGcsHorizontal, lcsOriginHorizontal, rotationMatrixHorizontal);
        Console.WriteLine($"Recomputed Initial Horizontal Detailed Position: E {recomputedHorizontal.X}, N {recomputedHorizontal.Y}, U {recomputedHorizontal.Z}");

        Console.WriteLine();

        // Vertical member example
        Point3D xDirectionVertical = new Point3D(0, 0, 1);
        Point3D zDirectionVertical = new Point3D(0, 1, 0);
        double[,] rotationMatrixVertical = CoordinateTransformer.ComputeRotationMatrix(xDirectionVertical, zDirectionVertical);

        Point3D lcsOriginVertical = new Point3D(500000, 300000, 100000);

        Point3D pointGcsVertical = CoordinateTransformer.LcsToGcs(detailingVectorLcs, lcsOriginVertical, rotationMatrixVertical);
        Console.WriteLine($"Vertical Detailed Position: E {pointGcsVertical.X}, N {pointGcsVertical.Y}, U {pointGcsVertical.Z}");
        Console.WriteLine($"Expected Vertical Detailed Position: E {499960}, N {300050}, U {99900}");
        Point3D recomputedVertical = CoordinateTransformer.GcsToLcs(pointGcsVertical, lcsOriginVertical, rotationMatrixVertical);
        Console.WriteLine($"Recomputed Initial Vertical Detailed Position: E {recomputedVertical.X}, N {recomputedVertical.Y}, U {recomputedVertical.Z}");
    }
}