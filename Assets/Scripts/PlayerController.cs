using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(BoxCollider))]
public class PlayerController : MonoBehaviour
{

   

    [SerializeField] private Rigidbody _rigidbody;
    [SerializeField] private FixedJoystick _joystick;
    [SerializeField] private Animator _animator;

    [SerializeField] private float _moveSpeed;

    public TextMeshProUGUI scoreText;     // UI Text to display score
    private int score = 0;     // Score counter

    private void FixedUpdate()
    {
        _rigidbody.velocity = new Vector3(_joystick.Horizontal * _moveSpeed, _rigidbody.velocity.y, _joystick.Vertical * _moveSpeed);

        if (_joystick.Horizontal != 0 || _joystick.Vertical != 0)
        {
            transform.rotation = Quaternion.LookRotation(_rigidbody.velocity);
            _animator.SetBool("isRunning", true);
        }
        else
            _animator.SetBool("isRunning", false);
    }


    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Respawn"))
        {
            score++;
            UpdateScoreUI();
            Destroy(other.gameObject);  
        }
    }

    void UpdateScoreUI()
    {
        if (scoreText != null)
        {
            scoreText.text = "Nector: " + score;
        }
    }
}