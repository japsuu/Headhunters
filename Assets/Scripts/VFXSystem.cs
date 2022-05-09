using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

public class VFXSystem : MonoBehaviour
{
    [SerializeField]
    private Transform effectsRoot;
    
    [AssetsOnly]
    [SerializeField]
    private GameObject bloodEffect;

    [SerializeField]
    private float defaultEffectDestroyTime = 3f;

    private void Start()
    {
        Spear.OnSpearHitSomething.AddListener(CreateHitEffect);
    }

    private void CreateHitEffect(Collision collision)
    {
        ContactPoint contact = collision.GetContact(0);
        
        GameObject effect = Instantiate(bloodEffect, contact.point, Quaternion.Euler(contact.normal), effectsRoot);
        ParticleSystem ps = effect.GetComponentInChildren<ParticleSystem>();

        Destroy(effect, ps == null ? defaultEffectDestroyTime : ps.main.duration);
    }
}
