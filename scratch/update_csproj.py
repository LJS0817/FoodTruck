import os

csproj_path = r'd:\Pro\FoodTruck\Assembly-CSharp.csproj'
with open(csproj_path, 'r', encoding='utf-8') as f:
    content = f.read()

lines = content.split('\n')
for i in range(len(lines)-1, -1, -1):
    if '<Compile Include=' in lines[i]:
        lines.insert(i+1, '    <Compile Include="Assets\\Scripts\\Game\\Market\\UpgradeManager.cs" />')
        lines.insert(i+2, '    <Compile Include="Assets\\Scripts\\Game\\Market\\UpgradeUIController.cs" />')
        break

with open(csproj_path, 'w', encoding='utf-8') as f:
    f.write('\n'.join(lines))
print('Appended missing files to csproj.')
