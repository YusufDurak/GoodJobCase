using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// VFX Manager with Object Pooling for optimal performance
/// Pools particle systems to avoid Instantiate/Destroy calls
/// </summary>
public class VFXManager : MonoBehaviour
{
    public static VFXManager Instance { get; private set; }
    
    [Header("Particle Prefabs")]
    [SerializeField] private ParticleSystem explosionPrefab;
    
    [Header("Pool Settings")]
    [SerializeField] private int initialPoolSize = 20;
    [SerializeField] private int maxPoolSize = 50;
    
    // Object pool for particle systems
    private Queue<ParticleSystem> particlePool = new Queue<ParticleSystem>();
    private List<ParticleSystem> activeParticles = new List<ParticleSystem>();
    private Transform poolParent;
    
    private void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Optional: persist across scenes
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        
        InitializePool();
    }
    
    /// <summary>
    /// Pre-allocate particle systems for optimal performance
    /// </summary>
    private void InitializePool()
    {
        if (explosionPrefab == null)
        {
            Debug.LogWarning("Explosion prefab not assigned to VFXManager!");
            return;
        }
        
        // Create pool parent
        poolParent = new GameObject("ParticlePool").transform;
        poolParent.SetParent(transform);
        
        // Pre-instantiate particle systems
        for (int i = 0; i < initialPoolSize; i++)
        {
            CreateNewParticle();
        }
    }
    
    /// <summary>
    /// Create a new particle system and add to pool
    /// </summary>
    private ParticleSystem CreateNewParticle()
    {
        ParticleSystem ps = Instantiate(explosionPrefab, poolParent);
        ps.gameObject.SetActive(false);
        
        // Disable auto-play
        var main = ps.main;
        main.playOnAwake = false;
        
        particlePool.Enqueue(ps);
        return ps;
    }
    
    /// <summary>
    /// Get a particle system from the pool
    /// </summary>
    private ParticleSystem GetParticle()
    {
        ParticleSystem ps;
        
        // Get from pool or create new if needed
        if (particlePool.Count > 0)
        {
            ps = particlePool.Dequeue();
        }
        else
        {
            // Pool exhausted - create new (but limit max size)
            if (activeParticles.Count < maxPoolSize)
            {
                ps = CreateNewParticle();
                particlePool.Dequeue(); // Remove it since we just added it
            }
            else
            {
                // Max pool size reached - reuse oldest
                Debug.LogWarning("Particle pool exhausted! Reusing oldest particle.");
                ps = activeParticles[0];
                ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            }
        }
        
        activeParticles.Add(ps);
        return ps;
    }
    
    /// <summary>
    /// Return a particle system to the pool
    /// </summary>
    private void ReturnParticle(ParticleSystem ps)
    {
        if (ps == null) return;
        
        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        ps.gameObject.SetActive(false);
        ps.transform.SetParent(poolParent);
        
        activeParticles.Remove(ps);
        particlePool.Enqueue(ps);
    }
    
    /// <summary>
    /// Play explosion effect at position with color (OPTIMIZED)
    /// </summary>
    public void PlayExplosion(Vector3 position, Color color)
    {
        if (explosionPrefab == null) return;
        
        // Get particle from pool
        ParticleSystem ps = GetParticle();
        
        // Configure
        ps.transform.position = position;
        ps.gameObject.SetActive(true);
        
        // Set color
        var main = ps.main;
        main.startColor = color;
        
        // Play
        ps.Play();
        
        // Return to pool after duration
        float duration = main.duration + main.startLifetime.constantMax;
        StartCoroutine(ReturnParticleAfterDelay(ps, duration));
    }
    
    /// <summary>
    /// Return particle to pool after delay
    /// </summary>
    private System.Collections.IEnumerator ReturnParticleAfterDelay(ParticleSystem ps, float delay)
    {
        yield return new WaitForSeconds(delay);
        ReturnParticle(ps);
    }
    
    /// <summary>
    /// Clear all active particles (useful for scene transitions)
    /// </summary>
    public void ClearAllParticles()
    {
        foreach (ParticleSystem ps in activeParticles.ToArray())
        {
            ReturnParticle(ps);
        }
    }
}