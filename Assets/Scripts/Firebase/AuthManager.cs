using System.Collections;
using Firebase;
using Firebase.Auth;
using TMPro;
using UnityEngine;

public class AuthManager : MonoBehaviour
{
    [SerializeField] private MainMenu mainMenuScript;

    // Firebase variables
    [Header("Firebase")]
    public DependencyStatus dependencyStatus;
    private FirebaseAuth auth;
    private FirebaseUser user;

    // Login variables
    [Header("Login")]
    [SerializeField] private TMP_InputField emailLoginField;
    [SerializeField] private TMP_InputField passwordLoginField;
    [SerializeField] private TMP_Text infoLoginText;

    // Register variables
    [Header("Register")]
    [SerializeField] private TMP_InputField usernameRegisterField;
    [SerializeField] private TMP_InputField emailRegisterField;
    [SerializeField] private TMP_InputField passwordRegisterField;
    [SerializeField] private TMP_InputField passwordRegisterVerifyField;
    [SerializeField] private TMP_Text infoRegisterText;

    private void Awake()
    {
        // Comprueba que todas las dependencias necesarias para Firebase están presentes
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>
        {
            dependencyStatus = task.Result;
            if (dependencyStatus == DependencyStatus.Available)
                // If they are avalible Initialize Firebase
                InitializeFirebase();
            else
                Debug.LogError("Could not resolve all Firebase dependencies: " + dependencyStatus);
        });
    }

    private void InitializeFirebase()
    {
        Debug.Log("Setting up Firebase Auth");
        auth = FirebaseAuth.DefaultInstance;
    }

    public void LoginButton()
    {
        StartCoroutine(Login(emailLoginField.text, passwordLoginField.text));
    }

    public void RegisterButton()
    {
        StartCoroutine(Register(emailRegisterField.text, passwordRegisterField.text, usernameRegisterField.text));
    }

    private IEnumerator Login(string _email, string _password)
    {
        // Llama a la función de autenticación de Firebase
        var LoginTask = auth.SignInWithEmailAndPasswordAsync(_email, _password);
        // Espera a que se complete la tarea
        yield return new WaitUntil(predicate: () => LoginTask.IsCompleted);

        if (LoginTask.Exception != null)
        {
            // Si hay errores, los gestiona
            Debug.LogWarning(message: $"Failed to register task with {LoginTask.Exception}");
            FirebaseException firebaseEx = LoginTask.Exception.GetBaseException() as FirebaseException;
            AuthError errorCode = (AuthError) firebaseEx.ErrorCode;

            string message = errorCode switch
            {
                AuthError.MissingEmail => "Email en blanco",
                AuthError.MissingPassword => "Contraseña en blanco",
                AuthError.WrongPassword => "Contraseña incorrecta",
                AuthError.InvalidEmail => "Email no válido",
                AuthError.UserNotFound => "La cuenta no existe",
                _ => "No se pudo iniciar sesión"
            };

            infoLoginText.text = message;
        }
        else
        {
            // El usuario ya está logeado. Obtiene los resultados
            user = LoginTask.Result;
            Debug.LogFormat("User signed in successfully: {0} ({1})", user.DisplayName, user.Email);
            infoLoginText.text = "";
            infoLoginText.text = "Sesión iniciada";
        }
    }

    private IEnumerator Register(string _email, string _password, string _username)
    {
        if (_username == "")
            infoRegisterText.text = "Nombre de usuario en blanco";
        else if (passwordRegisterField.text != passwordRegisterVerifyField.text)
            infoRegisterText.text = "Las contraseñas no coinciden";
        else
        {
            // Llama a la función de registro de Firebase
            var RegisterTask = auth.CreateUserWithEmailAndPasswordAsync(_email, _password);
            // Espera a que se complete la tarea
            yield return new WaitUntil(predicate: () => RegisterTask.IsCompleted);

            if (RegisterTask.Exception != null)
            {
                // Si hay errores, los gestiona
                Debug.LogWarning(message: $"Failed to register task with {RegisterTask.Exception}");
                FirebaseException firebaseEx = RegisterTask.Exception.GetBaseException() as FirebaseException;
                AuthError errorCode = (AuthError) firebaseEx.ErrorCode;

                string message = errorCode switch
                {
                    AuthError.MissingEmail => "Email en blanco",
                    AuthError.MissingPassword => "Contraseña en blanco",
                    AuthError.WeakPassword => "Contraseña poco segura",
                    AuthError.EmailAlreadyInUse => "El email ya está en uso",
                    _ => "No se pudo registrar la cuenta"
                };

                infoRegisterText.text = message;
            }
            else
            {
                // El usuario ya está creado. Obtiene los resultados
                user = RegisterTask.Result;

                if (user != null)
                {
                    // Crea un perfil y asigna su nombre de usuario
                    UserProfile profile = new UserProfile {DisplayName = _username};

                    // Llama a la función de Firebase de actualizar el nombre de usuario
                    var ProfileTask = user.UpdateUserProfileAsync(profile);
                    // Espera a que se complete la tarea
                    yield return new WaitUntil(predicate: () => ProfileTask.IsCompleted);

                    if (ProfileTask.Exception != null)
                    {
                        // Si hay errores, los gestiona
                        Debug.LogWarning(message: $"Failed to register task with {ProfileTask.Exception}");
                        FirebaseException firebaseEx = ProfileTask.Exception.GetBaseException() as FirebaseException;
                        AuthError errorCode = (AuthError) firebaseEx.ErrorCode;
                        infoRegisterText.text = "No se pudo asignar el nombre de usuario";
                    }
                    else
                    {
                        // El nombre de usuario ya está establecido. Vuelve a la pantalla de login
                        infoLoginText.text = "Registrado con éxito";
                        mainMenuScript.ShowLoginScreen();
                        infoRegisterText.text = "";
                    }
                }
            }
        }
    }
}