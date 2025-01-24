using UnityEngine;
using Obi;

public class FluidVolumeCalculator : MonoBehaviour
{
    public ObiSolver solver; // Reference to the ObiSolver handling the fluid simulation.
    public Collider container; // Reference to the container's collider.

    void Update()
    {
        float totalFluidMass = 0f;

        // Loop through all the particles that are currently active in the solver
        for (int i = 0; i < solver.activeParticleCount; i++)
        {
            int particleIndex = solver.simplices[i]; // Get the actual particle index from simplices
            Vector4 particlePosition = solver.positions[particleIndex];
            Vector3 worldPosition = solver.transform.TransformPoint(particlePosition);

            // Check if the particle is inside the container
            if (container.bounds.Contains(worldPosition))
            {
                // Add the particle's mass (if invMass > 0)
                totalFluidMass += solver.invMasses[particleIndex] > 0 ? 1f / solver.invMasses[particleIndex] : 0;
            }
        }

        Debug.Log("Total Fluid Mass in Container: " + totalFluidMass);
    }
}

