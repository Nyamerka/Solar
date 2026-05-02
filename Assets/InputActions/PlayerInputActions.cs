using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

public partial class PlayerInputActions : IInputActionCollection2, IDisposable
{
    public InputActionAsset asset { get; }

    public PlayerInputActions()
    {
        asset = InputActionAsset.FromJson(@"{
            ""name"": ""PlayerInputActions"",
            ""maps"": [
                {
                    ""name"": ""Player"",
                    ""id"": ""a1b2c3d4-e5f6-7890-abcd-ef1234567890"",
                    ""actions"": [
                        { ""name"": ""Move"", ""type"": ""Value"", ""id"": ""11111111-1111-1111-1111-111111111111"", ""expectedControlType"": ""Vector2"" },
                        { ""name"": ""Flash"", ""type"": ""Button"", ""id"": ""22222222-2222-2222-2222-222222222222"" },
                        { ""name"": ""Burst"", ""type"": ""Button"", ""id"": ""33333333-3333-3333-3333-333333333333"" },
                        { ""name"": ""Interact"", ""type"": ""Button"", ""id"": ""44444444-4444-4444-4444-444444444444"" },
                        { ""name"": ""Pause"", ""type"": ""Button"", ""id"": ""55555555-5555-5555-5555-555555555555"" }
                    ],
                    ""bindings"": [
                        { ""name"": ""WASD"", ""id"": ""aaa11111-aaaa-aaaa-aaaa-aaaaaaaaaaaa"", ""path"": ""2DVector"", ""action"": ""Move"", ""isComposite"": true, ""isPartOfComposite"": false },
                        { ""name"": ""up"", ""id"": ""aaa22222-aaaa-aaaa-aaaa-aaaaaaaaaaaa"", ""path"": ""<Keyboard>/w"", ""action"": ""Move"", ""isComposite"": false, ""isPartOfComposite"": true },
                        { ""name"": ""down"", ""id"": ""aaa33333-aaaa-aaaa-aaaa-aaaaaaaaaaaa"", ""path"": ""<Keyboard>/s"", ""action"": ""Move"", ""isComposite"": false, ""isPartOfComposite"": true },
                        { ""name"": ""left"", ""id"": ""aaa44444-aaaa-aaaa-aaaa-aaaaaaaaaaaa"", ""path"": ""<Keyboard>/a"", ""action"": ""Move"", ""isComposite"": false, ""isPartOfComposite"": true },
                        { ""name"": ""right"", ""id"": ""aaa55555-aaaa-aaaa-aaaa-aaaaaaaaaaaa"", ""path"": ""<Keyboard>/d"", ""action"": ""Move"", ""isComposite"": false, ""isPartOfComposite"": true },
                        { ""name"": """", ""id"": ""bbb11111-bbbb-bbbb-bbbb-bbbbbbbbbbbb"", ""path"": ""<Keyboard>/space"", ""action"": ""Flash"", ""isComposite"": false, ""isPartOfComposite"": false },
                        { ""name"": """", ""id"": ""ccc11111-cccc-cccc-cccc-cccccccccccc"", ""path"": ""<Keyboard>/q"", ""action"": ""Burst"", ""isComposite"": false, ""isPartOfComposite"": false },
                        { ""name"": """", ""id"": ""ddd11111-dddd-dddd-dddd-dddddddddddd"", ""path"": ""<Keyboard>/e"", ""action"": ""Interact"", ""isComposite"": false, ""isPartOfComposite"": false },
                        { ""name"": """", ""id"": ""eee11111-eeee-eeee-eeee-eeeeeeeeeeee"", ""path"": ""<Keyboard>/escape"", ""action"": ""Pause"", ""isComposite"": false, ""isPartOfComposite"": false }
                    ]
                }
            ],
            ""controlSchemes"": [
                {
                    ""name"": ""Keyboard"",
                    ""bindingGroup"": ""Keyboard"",
                    ""devices"": [
                        { ""devicePath"": ""<Keyboard>"", ""isOptional"": false },
                        { ""devicePath"": ""<Mouse>"", ""isOptional"": true }
                    ]
                }
            ]
        }");
        m_Player = asset.FindActionMap("Player", throwIfNotFound: true);
        m_Player_Move = m_Player.FindAction("Move", throwIfNotFound: true);
        m_Player_Flash = m_Player.FindAction("Flash", throwIfNotFound: true);
        m_Player_Burst = m_Player.FindAction("Burst", throwIfNotFound: true);
        m_Player_Interact = m_Player.FindAction("Interact", throwIfNotFound: true);
        m_Player_Pause = m_Player.FindAction("Pause", throwIfNotFound: true);
    }

    ~PlayerInputActions() { Dispose(); }

    public void Dispose()
    {
        UnityEngine.Object.Destroy(asset);
    }

    public InputBinding? bindingMask
    {
        get => asset.bindingMask;
        set => asset.bindingMask = value;
    }

    public System.Collections.Generic.IEnumerator<InputAction> GetEnumerator() => asset.GetEnumerator();
    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();

    public bool Contains(InputAction action) => asset.Contains(action);
    public void Enable() => asset.Enable();
    public void Disable() => asset.Disable();

    public InputAction FindAction(string actionNameOrId, bool throwIfNotFound = false)
        => asset.FindAction(actionNameOrId, throwIfNotFound);

    public int FindBinding(InputBinding bindingMask, out InputAction action)
        => asset.FindBinding(bindingMask, out action);

    public System.Collections.Generic.IEnumerable<InputBinding> bindings => asset.bindings;
    public ReadOnlyArray<InputDevice>? devices
    {
        get => asset.devices;
        set => asset.devices = value;
    }
    public ReadOnlyArray<InputControlScheme> controlSchemes => asset.controlSchemes;

    // --- Player Action Map ---
    private InputActionMap m_Player;
    private InputAction m_Player_Move;
    private InputAction m_Player_Flash;
    private InputAction m_Player_Burst;
    private InputAction m_Player_Interact;
    private InputAction m_Player_Pause;

    public PlayerActions Player => new PlayerActions(this);

    public struct PlayerActions
    {
        private PlayerInputActions m_Wrapper;
        public PlayerActions(PlayerInputActions wrapper) { m_Wrapper = wrapper; }

        public InputAction Move => m_Wrapper.m_Player_Move;
        public InputAction Flash => m_Wrapper.m_Player_Flash;
        public InputAction Burst => m_Wrapper.m_Player_Burst;
        public InputAction Interact => m_Wrapper.m_Player_Interact;
        public InputAction Pause => m_Wrapper.m_Player_Pause;

        public InputActionMap Get() => m_Wrapper.m_Player;
        public void Enable() => Get().Enable();
        public void Disable() => Get().Disable();
        public static implicit operator InputActionMap(PlayerActions set) => set.Get();
    }
}
