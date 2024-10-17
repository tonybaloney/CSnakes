
def test_number() -> int:
    return 52

# The following function will introduce a method named `ReloadModule` in the C#
# proxy's interface that hides an identically named and inherited method
# from `IReloadableModuleImport`. The source generator should appropriately add
# the `new` keyword so the following warning is never emitted:
#
# > warning CS0108: 'ITestReload.ReloadModule()' hides inherited member
# >   'IReloadableModuleImport.ReloadModule()'. Use the new keyword if hiding
# >   was intended.

def reload_module():
    pass
