using UnityEngine;
using System.Collections.Generic;

public class VFXManager : MonoBehaviour
{
    public static VFXManager Instance { get; private set; }
    
    [Header("Particle Prefabs")]
    [SerializeField] private ParticleSystem explosionPrefab;
    
    [Header("Pool Settings")]
    [SerializeField] private int initialPoolSize = 20;
    [SerializeField] private int maxPoolSize = 50;
    
    private Queue<ParticleSystem> particlePool = new Queue<ParticleSystem>();
    private List<ParticleSystem> activeParticles = new List<ParticleSystem>();
    private Transform poolParent;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        
        InitializePool();
    }
    
    private void InitializePool()
    {
        if (explosionPrefab == null) return;
        
        poolParent = new GameObject("ParticlePool").transform;
        poolParent.SetParent(transform);
        
        for (int i = 0; i < initialPoolSize; i++)
        {
            CreateNewParticle();
        }
    }
    
    private ParticleSystem CreateNewParticle()
    {
        ParticleSystem ps = Instantiate(explosionPrefab, poolParent);
        ps.gameObject.SetActive(false);
        
        var main = ps.main;
        main.playOnAwake = false;
        
        particlePool.Enqueue(ps);
        return ps;
    }
    
    private ParticleSystem GetParticle()
    {
        ParticleSystem ps;
        
        if (particlePool.Count > 0)
        {
            ps = particlePool.Dequeue();
        }
        else
        {
            if (activeParticles.Count < maxPoolSize)
            {
                ps = CreateNewParticle();
                particlePool.Dequeue();
            }
            else
            {
                ps = activeParticles[0];
                ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            }
        }
        
        activeParticles.Add(ps);
        return ps;
    }
    
    private void ReturnParticle(ParticleSystem ps)
    {
        if (ps == null) return;
        
        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        ps.gameObject.SetActive(false);
        ps.transform.SetParent(poolParent);
        
        activeParticles.Remove(ps);
        particlePool.Enqueue(ps);
    }
    
    public void PlayExplosion(Vector3 position, Color color)
    {
        if (explosionPrefab == null) return;
        
        ParticleSystem ps = GetParticle();
        
        ps.transform.position = position;
        ps.gameObject.SetActive(true);
        
        var main = ps.main;
        main.startColor = color;
        
        ps.Play();
        
        float duration = main.duration + main.startLifetime.constantMax;
        StartCoroutine(ReturnParticleAfterDelay(ps, duration));
    }
    
    private System.Collections.IEnumerator ReturnParticleAfterDelay(ParticleSystem ps, float delay)
    {
        yield return new WaitForSeconds(delay);
        ReturnParticle(ps);
    }
    
    public void ClearAllParticles()
    {
        foreach (ParticleSystem ps in activeParticles.ToArray())
        {
            ReturnParticle(ps);
        }
    }
}
