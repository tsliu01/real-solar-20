Horse Drawn Games Remote Debug Version 1.0.3050 Beta Release
============================================================

Quick Start
-----------

1.  Place the `RemoteDebugServer` prefab (in Plugins/HdgRemoteDebug/Runtime) somewhere in a scene you would like to debug.
2.  Deploy your game to your device and run it.
3.  Bring up the remote debug window by clicking on "Window > Hdg Remote Debug".
4.  Click the "Active Player" button and the device should appear in the list. Select it.


Troubleshooting the connection
------------------------------

If you have troubles connecting to the device, please check the following:

   * Wait until your game has started and the scene with the RemoteDebugServer prefab or component has loaded. Only then
     will the device appear in the Remote Debug Active Player dropdown.

   * Ensure that your device and the computer where you are running are connected to the same network. The debug window
     talks to the device via a network connection.

   * If a firewall is enabled on your device or your network, it may be blocking connections. Remote Debug uses TCP and
     UDP and makes connections on port 12000. Either disable the firewall or allow TCP and UDP connections on port 12000.

If you have troubles with some properties not appearing in the window, please check the following:

   * Ensure you are using the Mono scripting backend rather than IL2CPP.

   * Set the Stripping Level to "Disabled".

Because Hdg Remote Debug uses reflection to find properties and fields to send back to the client, if stripping is enabled
it is possible that only a small number of properties (or even no properties) will be found. If no code references a
property Unity will strip it.

IL2CPP is particularly aggressive with stripping. If you want to use IL2CPP and some properties are not appearing, you can
use a link.xml file to ensure those properties are not stripped. For example if you want to make sure that all UnityEngine
properties are not stripped, you can put the following in link.xml:

   <linker>
      <assembly fullname="UnityEngine" preserve="all" />
   </linker>


Notes and limitations
---------------------

   * Hdg Remote Debug serialises fields on components itself using reflection. Beware that stripping can cause some fields
     to not appear in the window! See "Troubleshooting" above.

   * References are currently not able to be changed.

   * Custom inspectors are not shown (Hdg Remote Debug draws its own custom hierarchy and inspector views).

   * Sometimes changing a field may not seem to have any effect in game, but it works when you change it using the
     built-in inspector in the editor. This is probably because the field has a custom inspector that does some other
     work to update the object. As an example, when you update the text in a Text component, it won't change until you
     turn the text off and on again so it can rebuild its vertices; ordinarily the custom Text inspector updates the
     vertices.

   * The server will mark its GameObject as DontDestroyOnLoad so it will remain around in memory forever.

   * Because of a bug in Unity, manually loaded resources will also show up in the hierarchy at the moment. This bug
     has been fixed in 5.3.4p3. This is due to a bug in `SceneManagement.GetRootGameObjects` which Remote Debug uses
     to gather the list of root game objects in the current scene.

   * Currently objects that have had DontDestroyOnLoad called on them don't show up. I am investigating ways to get
     those objects to show up.

   * There is a performance penalty when running the server with a live connection: every second the server sends a
     list of current GameObjects back to the editor, and when you select a GameObject it will also send back all the
     components and serialised fields. These are somewhat expensive operations and unfortunately also result in some
     memory allocations, as the server uses reflection to find the fields and Mono makes allocations when using many
     of the reflection methods. Therefore it is best to disable the Remote Debug server when doing any profiling.

If there are any other problems, click "Debug" in the Remote Debug toolbar, try to connect again, and email any log messages
along with the version you are using to:

   sam@horsedrawngames.com

Thank you for your interest in Hdg Remote Debug!
