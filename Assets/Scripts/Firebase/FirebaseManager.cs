using System.Collections;
using Firebase;
using Firebase.Auth;
using TMPro;
using UnityEngine;

public class FirebaseManager : MonoBehaviour
{
    // Instancia del FirebaseManager a la que accederemos desde fuera
    private static FirebaseManager instance = null;

    public static FirebaseManager GetInstance()
    {
        return instance;
    }

    [HideInInspector] public MainMenu mainMenuScript;

    // Firebase variables
    [Header("Firebase")] public DependencyStatus dependencyStatus;
    public FirebaseAuth auth;
    public FirebaseUser user;

    private void Awake()
    {
        // Si no hay ningún FirebaseManager instanciado
        if (instance == null)
            instance = this;
        // Si hay alguno instanciado pero no soy yo me destruyo porque no soy necesario
        else if (instance != this)
            Destroy(gameObject);
        // Indico que no debo destruirme cuando recargue los niveles
        DontDestroyOnLoad(gameObject);

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
        StartCoroutine(Login(mainMenuScript.emailLoginField.text, mainMenuScript.passwordLoginField.text));
    }

    public void RegisterButton()
    {
        StartCoroutine(Register(mainMenuScript.emailRegisterField.text, mainMenuScript.passwordRegisterField.text,
            mainMenuScript.usernameRegisterField.text));
    }

    public void SignOutButton()
    {
        auth.SignOut();
        mainMenuScript.ShowLoginScreen();
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

            mainMenuScript.infoLoginText.text = message;
        }
        else
        {
            // El usuario ya está logeado. Obtiene los resultados
            user = LoginTask.Result;
            Debug.LogFormat("User signed in successfully: {0} ({1})", user.DisplayName, user.Email);
            mainMenuScript.infoLoginText.text = "";
            mainMenuScript.infoLoginText.text = "Sesión iniciada";
            mainMenuScript.ShowTitleScreen();
        }
    }

    private IEnumerator Register(string _email, string _password, string _username)
    {
        if (_username == "")
            mainMenuScript.infoRegisterText.text = "Nombre de usuario en blanco";
        else if (mainMenuScript.passwordRegisterField.text != mainMenuScript.passwordRegisterVerifyField.text)
            mainMenuScript.infoRegisterText.text = "Las contraseñas no coinciden";
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

                mainMenuScript.infoRegisterText.text = message;
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
                        mainMenuScript.infoRegisterText.text = "No se pudo asignar el nombre de usuario";
                    }
                    else
                    {
                        // El nombre de usuario ya está establecido. Vuelve a la pantalla de login
                        mainMenuScript.infoLoginText.text = "Registrado con éxito";
                        mainMenuScript.ShowLoginScreen();
                        mainMenuScript.infoRegisterText.text = "";
                    }
                }
            }
        }
    }
}