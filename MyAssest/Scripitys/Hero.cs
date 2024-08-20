using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.GraphicsBuffer;

public class Hero : MonoBehaviour
{
    [Header("Atributos hero")]
    public string heroName;
    public CardType heroType;
    public int attackPower;
    [Header("lifeHero")]
    public Image heroImage; // Imagem do herói
    public Canvas Canvas; // Imagem do herói
    public Image heroImageColor; // Imagem do herói
    public Slider healthBar; public float maxHealth = 100f;
    private float currentHealth;
    [Header("fire hero")]
    public GameObject projectilePrefab; // Prefab do tiro
    public float projectileSpeedTime = 1f;
    public GameObject fxPrefab;
    public GameObject teste;
    Vector3 originalPosition;
    Color originalColor;
    void Start()
    {
        currentHealth = maxHealth;
         originalPosition = heroImage.rectTransform.localPosition;
         originalColor = heroImageColor.color;

    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.L))
        {
            Attack(teste.transform);
        }

    }
    public void TakeDamage(float damage)
    {
        StartCoroutine(AnimateDamage(damage));
    }
    public void Attack(Transform target)
    {
        StartCoroutine(MoveProjectile(target, projectileSpeedTime));
    }

    private IEnumerator MoveProjectile(Transform target, float travelTime)
    {
        // Instancia o projétil na posição do herói, sem rotação inicial
        GameObject projectile = Instantiate(projectilePrefab, transform.position, Quaternion.identity);
        projectile.transform.SetParent(Canvas.transform);

        // Calcula a direção do projétil em relação ao alvo
        Vector3 direction = (target.position - projectile.transform.position).normalized;

        // Calcula o ângulo necessário para o projétil apontar na direção do alvo
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        // Aplica a rotação ao projétil. Aqui, ajustamos o ângulo para garantir que a frente da imagem fique voltada para o alvo
        projectile.transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle));

        // Define a escala do projétil para 30% do tamanho do herói
        projectile.transform.localScale = transform.localScale * 0.3f;

        // Calcula a distância entre o projétil e o alvo
        float distance = Vector3.Distance(projectile.transform.position, target.position);

        // Calcula a velocidade necessária para o projétil percorrer essa distância no tempo especificado
        float speed = distance / travelTime;

        while (Vector3.Distance(projectile.transform.position, target.position) > 0.1f)
        {
            // Move o projétil na direção do alvo com a velocidade calculada
            projectile.transform.position = Vector3.MoveTowards(projectile.transform.position, target.position, speed * Time.deltaTime);
            yield return null;
        }

        // Quando o projétil chega perto do alvo, destrói o projétil e cria o FX
        var fx = Instantiate(fxPrefab, projectile.transform.position, Quaternion.identity);
        fx.transform.SetParent(Canvas.transform);
        Destroy(fx, 2);
        Destroy(projectile);
    }

    private IEnumerator AnimateDamage(float damage)
    {
        // Passo 1: Faz a imagem do herói ficar vermelha por 0.2 segundos
        heroImageColor.color = Color.red;

        // Passo 2: Faz a imagem tremer para a direita e esquerda

        for (int i = 0; i < 2; i++)
        {
            heroImage.rectTransform.localPosition = originalPosition + new Vector3(1.5f, 0, 0);
            yield return new WaitForSeconds(0.05f);
            heroImage.rectTransform.localPosition = originalPosition + new Vector3(-1.5f, 0, 0);
            yield return new WaitForSeconds(0.05f);
        }

        // Volta à posição original
        heroImage.rectTransform.localPosition = originalPosition;
        yield return new WaitForSeconds(0.2f);
        heroImageColor.color = originalColor;

        // Passo 3: Diminui o fillAmount da barra de vida
        currentHealth -= damage;
        healthBar.value = currentHealth / maxHealth;

        yield return null;
    }
}