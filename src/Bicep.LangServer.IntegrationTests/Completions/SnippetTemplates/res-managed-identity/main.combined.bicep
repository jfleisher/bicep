resource managedIdentity 'Microsoft.ManagedIdentity/userAssignedIdentities@2018-11-30' = {
  name: 'testManagedIdentity'
  location: resourceGroup().location
}
