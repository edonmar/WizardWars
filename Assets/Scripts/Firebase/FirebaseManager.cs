using System.Collections;
using Firebase;
using Firebase.Auth;
using Firebase.Database;
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

    private GameManager gameManager;
    [HideInInspector] public MainMenu mainMenuScript;

    // Firebase variables
    [Header("Firebase")] public DependencyStatus dependencyStatus;
    public FirebaseAuth auth;
    public FirebaseUser user;
    public DatabaseReference dbReference;

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


        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();

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
        dbReference = FirebaseDatabase.DefaultInstance.RootReference;
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

    public void SaveData(int time, string rooms, int castedSpells, int castedMagicksTotal, string magickDetails)
    {
        StartCoroutine(UpdateUsernameAuth(user.DisplayName));
        StartCoroutine(UpdateUsernameDatabase(user.DisplayName));

        StartCoroutine(UpdateTime(time));
        StartCoroutine(UpdateRooms(rooms));
        StartCoroutine(UpdateSpells(castedSpells));
        StartCoroutine(UpdateMagicks(castedMagicksTotal));
        StartCoroutine(UpdateMagickDetails(magickDetails));
    }

    private IEnumerator Login(string email, string password)
    {
        // Llama a la función de autenticación de Firebase
        var loginTask = auth.SignInWithEmailAndPasswordAsync(email, password);
        // Espera a que se complete la tarea
        yield return new WaitUntil(predicate: () => loginTask.IsCompleted);

        if (loginTask.Exception != null)
        {
            // Si hay errores, los gestiona
            Debug.LogWarning(message: $"Failed to register task with {loginTask.Exception}");
            FirebaseException firebaseEx = loginTask.Exception.GetBaseException() as FirebaseException;
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
            user = loginTask.Result;
            Debug.LogFormat("User signed in successfully: {0} ({1})", user.DisplayName, user.Email);
            mainMenuScript.infoLoginText.text = "";
            mainMenuScript.infoLoginText.text = "Sesión iniciada";
            mainMenuScript.ShowTitleScreen();
        }
    }

    private IEnumerator Register(string email, string password, string username)
    {
        if (username == "")
            mainMenuScript.infoRegisterText.text = "Nombre de usuario en blanco";
        else if (mainMenuScript.passwordRegisterField.text != mainMenuScript.passwordRegisterVerifyField.text)
            mainMenuScript.infoRegisterText.text = "Las contraseñas no coinciden";
        else
        {
            // Llama a la función de registro de Firebase
            var registerTask = auth.CreateUserWithEmailAndPasswordAsync(email, password);
            // Espera a que se complete la tarea
            yield return new WaitUntil(predicate: () => registerTask.IsCompleted);

            if (registerTask.Exception != null)
            {
                // Si hay errores, los gestiona
                Debug.LogWarning(message: $"Failed to register task with {registerTask.Exception}");
                FirebaseException firebaseEx = registerTask.Exception.GetBaseException() as FirebaseException;
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
                user = registerTask.Result;

                if (user != null)
                {
                    // Crea un perfil y asigna su nombre de usuario
                    UserProfile profile = new UserProfile {DisplayName = username};

                    // Llama a la función de Firebase de actualizar el nombre de usuario
                    var profileTask = user.UpdateUserProfileAsync(profile);
                    // Espera a que se complete la tarea
                    yield return new WaitUntil(predicate: () => profileTask.IsCompleted);

                    if (profileTask.Exception != null)
                    {
                        // Si hay errores, los gestiona
                        Debug.LogWarning(message: $"Failed to register task with {profileTask.Exception}");
                        FirebaseException firebaseEx = profileTask.Exception.GetBaseException() as FirebaseException;
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

    private IEnumerator UpdateUsernameAuth(string username)
    {
        // Creado un perfil de usuario y asignado el nombre de usuario
        UserProfile profile = new UserProfile {DisplayName = username};

        // Llama la función de Firebase de actualizar perfil de usuario, pasando el perfil con el nombre de usuario
        var profileTask = user.UpdateUserProfileAsync(profile);
        // Espera a que se complete la tarea
        yield return new WaitUntil(predicate: () => profileTask.IsCompleted);

        if (profileTask.Exception != null)
            Debug.LogWarning(message: $"Failed to register task with {profileTask.Exception}");
    }

    private IEnumerator UpdateUsernameDatabase(string username)
    {
        // Guarda en la base de datos el nombre de usuairo del usuario logeado actualmente
        var dbTask = dbReference.Child("users").Child(user.UserId).Child("username").SetValueAsync(username);

        // Espera a que se complete la tarea
        yield return new WaitUntil(predicate: () => dbTask.IsCompleted);

        if (dbTask.Exception != null)
            Debug.LogWarning(message: $"Failed to register task with {dbTask.Exception}");
    }

    // Asigna datos para el usuario logeado actualmente
    private IEnumerator UpdateTime(int time)
    {
        var dbTask = dbReference.Child("users").Child(user.UserId).Child("time").SetValueAsync(time);

        yield return new WaitUntil(predicate: () => dbTask.IsCompleted);

        if (dbTask.Exception != null)
            Debug.LogWarning(message: $"Failed to register task with {dbTask.Exception}");
    }

    private IEnumerator UpdateRooms(string rooms)
    {
        var dbTask = dbReference.Child("users").Child(user.UserId).Child("rooms").SetValueAsync(rooms);

        yield return new WaitUntil(predicate: () => dbTask.IsCompleted);

        if (dbTask.Exception != null)
            Debug.LogWarning(message: $"Failed to register task with {dbTask.Exception}");
    }

    private IEnumerator UpdateSpells(int spells)
    {
        var dbTask = dbReference.Child("users").Child(user.UserId).Child("spells").SetValueAsync(spells);

        yield return new WaitUntil(predicate: () => dbTask.IsCompleted);

        if (dbTask.Exception != null)
            Debug.LogWarning(message: $"Failed to register task with {dbTask.Exception}");
    }

    private IEnumerator UpdateMagicks(int magicks)
    {
        var dbTask = dbReference.Child("users").Child(user.UserId).Child("magicks").SetValueAsync(magicks);

        yield return new WaitUntil(predicate: () => dbTask.IsCompleted);

        if (dbTask.Exception != null)
            Debug.LogWarning(message: $"Failed to register task with {dbTask.Exception}");
    }

    private IEnumerator UpdateMagickDetails(string magickDetails)
    {
        var dbTask = dbReference.Child("users").Child(user.UserId).Child("magickDetails").SetValueAsync(magickDetails);

        yield return new WaitUntil(predicate: () => dbTask.IsCompleted);

        if (dbTask.Exception != null)
            Debug.LogWarning(message: $"Failed to register task with {dbTask.Exception}");
    }
}