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
    public Image heroImage; // Imagem do her�i
    public Canvas Canvas; // Imagem do her�i
    public Image heroImageColor; // Imagem do her�i
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
        // Instancia o proj�til na posi��o do her�i, sem rota��o inicial
        GameObject projectile = Instantiate(projectilePrefab, transform.position, Quaternion.identity);
        projectile.transform.SetParent(Canvas.transform);

        // Calcula a dire��o do proj�til em rela��o ao alvo
        Vector3 direction = (target.position - projectile.transform.position).normalized;

        // Calcula o �ngulo necess�rio para o proj�til apontar na dire��o do alvo
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        // Aplica a rota��o ao proj�til. Aqui, ajustamos o �ngulo para garantir que a frente da imagem fique voltada para o alvo
        projectile.transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle));

        // Define a escala do proj�til para 30% do tamanho do her�i
        projectile.transform.localScale = transform.localScale * 0.3f;

        // Calcula a dist�ncia entre o proj�til e o alvo
        float distance = Vector3.Distance(projectile.transform.position, target.position);

        // Calcula a velocidade necess�ria para o proj�til percorrer essa dist�ncia no tempo especificado
        float speed = distance / travelTime;

        while (Vector3.Distance(projectile.transform.position, target.position) > 0.1f)
        {
            // Move o proj�til na dire��o do alvo com a velocidade calculada
            projectile.transform.position = Vector3.MoveTowards(projectile.transform.position, target.position, speed * Time.deltaTime);
            yield return null;
        }

        // Quando o proj�til chega perto do alvo, destr�i o proj�til e cria o FX
        var fx = Instantiate(fxPrefab, projectile.transform.position, Quaternion.identity);
        fx.transform.SetParent(Canvas.transform);
        Destroy(fx, 2);
        Destroy(projectile);
    }

    private IEnumerator AnimateDamage(float damage)
    {
        // Passo 1: Faz a imagem do her�i ficar vermelha por 0.2 segundos
        heroImageColor.color = Color.red;

        // Passo 2: Faz a imagem tremer para a direita e esquerda

        for (int i = 0; i < 2; i++)
        {
            heroImage.rectTransform.localPosition = originalPosition + new Vector3(1.5f, 0, 0);
            yield return new WaitForSeconds(0.05f);
            heroImage.rectTransform.localPosition = originalPosition + new Vector3(-1.5f, 0, 0);
            yield return new WaitForSeconds(0.05f);
        }

        // Volta � posi��o original
        heroImage.rectTransform.localPosition = originalPosition;
        yield return new WaitForSeconds(0.2f);
        heroImageColor.color = originalColor;

        // Passo 3: Diminui o fillAmount da barra de vida
        currentHealth -= damage;
        healthBar.value = currentHealth / maxHealth;

        yield return null;
    }
}