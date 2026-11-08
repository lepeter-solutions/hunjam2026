"""Generate authored static backdrop assets and install only their scene root.
Run from repository root. Does not start Unity. Existing gameplay objects are preserved.
"""
import math, pathlib, re, struct, uuid, random

ROOT = pathlib.Path(__file__).resolve().parents[1]
GAME = ROOT / 'My project/Assets/_Game'
SCENE = GAME / 'Scenes/Manhattan.unity'
OUT = GAME / 'Art/ManhattanSurroundings'
NS = uuid.UUID('81527f03-3566-4e85-af99-30ddcb364e07')
SEA = -186.0

def guid(name): return uuid.uuid5(NS, name).hex
def write(path, text):
    if path != SCENE:
        text = '\n'.join(line.rstrip() for line in text.splitlines()) + '\n'
    path.write_text(text, encoding='utf-8', newline='\n')
def meta(path, kind='asset'):
    importer = 'NativeFormatImporter:\n  externalObjects: {}\n  mainObjectFileID: 4300000\n' if kind == 'mesh' else ''
    write(path.with_name(path.name+'.meta'), f'fileFormatVersion: 2\nguid: {guid(path.name)}\n'+importer)
def vec(v): return '{x: %.6f, y: %.6f, z: %.6f}' % tuple(v)
def smooth(t):
    t=max(0,min(1,t)); return t*t*(3-2*t)
def relief(x,z):
    return (.5+.24*math.sin(x*.0023+math.sin(z*.003))+.16*math.sin(z*.0041-x*.0018)
            +.07*math.sin(x*.011+z*.007)+.03*math.sin(z*.031-x*.019))
def margin(x,z):
    west = 5+55*math.sin(z*.003)+22*math.sin(z*.011)-x
    east = x-(900+80*math.sin(z*.0025)+25*math.sin(z*.009))
    north = z-(470+75*math.sin(x*.004)+25*math.sin(x*.012))
    island = 190*(1-math.sqrt(((x+600)/490)**2+((z+1500)/270)**2))
    return max(west,east,north,island)
def height(x,z):
    m=margin(x,z)
    upland=smooth((m-100)/900)
    amplitude=105 if x<0 or z>500 else 52
    # Fade the outer land into an irregular coast before the rectangular mesh grid ends.
    angle=math.atan2(z-600,x-400)
    coast=3100+100*math.sin(angle*5)+70*math.sin(angle*11)
    outer=smooth((coast-math.hypot(x-400,z-600))/280)
    return SEA-3+outer*smooth((m+25)/65)*(7+amplitude*upland*relief(x,z))
def normal(x,z):
    n=(height(x-1,z)-height(x+1,z),2,height(x,z-1)-height(x,z+1))
    d=math.sqrt(sum(a*a for a in n)); return tuple(a/d for a in n)

scene=SCENE.read_text(encoding='utf8')
template=re.search(r'--- !u!43 &1485530721\n(.*?)(?=--- !u!)',scene,re.S)[1]
OUT.mkdir(exist_ok=True)
write(OUT.with_name(OUT.name+'.meta'),f'fileFormatVersion: 2\nguid: {guid("folder")}\nfolderAsset: yes\nDefaultImporter:\n  externalObjects: {{}}\n')

def mesh(name,vertices,indices):
    lo=[min(v[0][i] for v in vertices) for i in range(3)]
    hi=[max(v[0][i] for v in vertices) for i in range(3)]
    center=vec([(a+b)/2 for a,b in zip(lo,hi)])
    extent=vec([(b-a)/2 for a,b in zip(lo,hi)])
    data=b''.join(struct.pack('<10f',*p,*n,*c,1) for p,n,c in vertices)
    channels=''
    for i in range(14):
        offset,dimension={0:(0,3),1:(12,3),3:(24,4)}.get(i,(0,0))
        channels+=f'    - stream: 0\n      offset: {offset}\n      format: 0\n      dimension: {dimension}\n'
    text=template
    text=re.sub(r'm_Name: .*',f'm_Name: {name}',text,count=1)
    text=re.sub(r'indexCount: \d+',f'indexCount: {len(indices)}',text,count=1)
    text=re.sub(r'vertexCount: \d+',f'vertexCount: {len(vertices)}',text,count=1)
    text=re.sub(r'm_Center: .*',f'm_Center: {center}',text)
    text=re.sub(r'm_Extent: .*',f'm_Extent: {extent}',text)
    text=re.sub(r'm_IndexBuffer: .*',f'm_IndexBuffer: {struct.pack("<"+"H"*len(indices),*indices).hex()}',text)
    vertex=f'  m_VertexData:\n    serializedVersion: 3\n    m_VertexCount: {len(vertices)}\n    m_Channels:\n{channels}    m_DataSize: {len(data)}\n    _typelessdata: {data.hex()}\n'
    text=re.sub(r'  m_VertexData:.*?(?=  m_CompressedMesh:)',vertex,text,flags=re.S)
    text=text.replace('m_IsReadable: 1','m_IsReadable: 0')
    path=OUT/(name+'.asset'); write(path,'%YAML 1.1\n%TAG !u! tag:unity3d.com,2011:\n--- !u!43 &4300000\n'+text); meta(path,'mesh')
    return (name,guid(path.name),len(indices)//3)

meshes=[]
for tz in range(4):
    for tx in range(4):
        vertices=[]; indices=[]; cells=48; size=1800
        for j in range(cells+1):
            for i in range(cells+1):
                x=-3200+tx*size+i*size/cells; z=-3000+tz*size+j*size/cells
                h=height(x,z); n=normal(x,z); m=margin(x,z)
                variation=relief(x*5,z*5)
                grass=(.16+variation*.065,.19+variation*.07,.13+variation*.05)
                rock=(.30,.29,.255)
                mix=smooth((.96-n[1])*12)+.2*(1-smooth(m/120))
                mix=min(1,mix)
                c=tuple(a*(1-mix)+b*mix for a,b in zip(grass,rock))
                vertices.append(((x,h,z),n,c))
        for j in range(cells):
            for i in range(cells):
                k=j*(cells+1)+i
                if max(vertices[a][0][1] for a in (k,k+1,k+cells+1,k+cells+2))<SEA-.2: continue
                indices.extend((k,k+cells+1,k+1,k+1,k+cells+1,k+cells+2))
        if indices: meshes.append(mesh(f'Coast_{tx}_{tz}',vertices,indices))

# One continuous disk replaces the visible square water tiles; no runtime tessellation.
vertices=[((400,SEA,-50),(0,1,0),(.1,.15,.2))]; indices=[]
for i in range(256):
    a=i*math.tau/256
    vertices.append(((400+18000*math.cos(a),SEA,-50+18000*math.sin(a)),(0,1,0),(.1,.15,.2)))
for i in range(256): indices.extend((0,(i+1)%256+1,i+1))
water=mesh('HarborWater',vertices,indices)

# Baked low-rise neighbourhoods beyond the water, with roof height/footprint variation.
rng=random.Random(20261108); vertices=[]; indices=[]; buildings=0
def box(x,y,z,w,h,d,color):
    corners=[(x-w/2,y,z-d/2),(x+w/2,y,z-d/2),(x+w/2,y,z+d/2),(x-w/2,y,z+d/2),
             (x-w/2,y+h,z-d/2),(x+w/2,y+h,z-d/2),(x+w/2,y+h,z+d/2),(x-w/2,y+h,z+d/2)]
    for face,n in [((0,4,5,1),(0,0,-1)),((1,5,6,2),(1,0,0)),((2,6,7,3),(0,0,1)),((3,7,4,0),(-1,0,0)),((4,7,6,5),(0,1,0))]:
        k=len(vertices)
        vertices.extend((corners[a],n,tuple(c*(.82 if n[1] else 1) for c in color)) for a in face)
        indices.extend((k,k+1,k+2,k,k+2,k+3))
for z in range(-950,1500,65):
    for x in range(-650,1700,60):
        m=margin(x,z)
        if not 65<m<420 or rng.random()<.28: continue
        x+=rng.uniform(-7,7); z1=z+rng.uniform(-7,7)
        h=rng.uniform(8,24)*(1.7 if rng.random()<.09 else 1)
        shade=rng.uniform(.22,.39)
        box(x,height(x,z1)-1,z1,rng.uniform(18,33),h,rng.uniform(22,40),(shade,shade*.94,shade*.86));buildings+=1
meshes.append(mesh('DistantNeighbourhoods',vertices,indices))

for shader in ('ManhattanLandscape','ManhattanHarbor'):
    path=GAME/'Art/Materials'/(shader+'.shader'); meta(path)
    mat=OUT/(shader+'.mat')
    props='    - _HazeDistance: 2400\n' if shader.endswith('Landscape') else '    - _Unused: 0\n'
    write(mat,f'''%YAML 1.1
%TAG !u! tag:unity3d.com,2011:
--- !u!21 &2100000
Material:
  serializedVersion: 8
  m_ObjectHideFlags: 0
  m_Name: {shader}
  m_Shader: {{fileID: 4800000, guid: {guid(path.name)}, type: 3}}
  m_ValidKeywords: []
  m_InvalidKeywords: []
  m_LightmapFlags: 4
  m_EnableInstancingVariants: 1
  m_CustomRenderQueue: -1
  stringTagMap: {{}}
  disabledShaderPasses: []
  m_SavedProperties:
    serializedVersion: 3
    m_TexEnvs: []
    m_Ints: []
    m_Floats:
{props}    m_Colors:
    - _HazeColor: {{r: 0.49, g: 0.53, b: 0.58, a: 1}}
    - _WaterColor: {{r: 0.055, g: 0.115, b: 0.15, a: 1}}
''');meta(mat)
script=GAME/'Scripts/World/ManhattanBackdrop.cs';meta(script)

# Use dedicated IDs, updating only this generated hierarchy on subsequent runs.
base=8800110800000000
scene=re.sub(r'--- !u!\d+ &88001108\d+\n.*?(?=--- !u!|\Z)','',scene,flags=re.S)
scene=re.sub(r'  - \{fileID: 88001108\d+\}\n','',scene)
def header(typ,ident,kind):
    return f'--- !u!{typ} &{ident}\n{kind}:\n  m_ObjectHideFlags: 0\n  m_CorrespondingSourceObject: {{fileID: 0}}\n  m_PrefabInstance: {{fileID: 0}}\n  m_PrefabAsset: {{fileID: 0}}\n'
def obj(ident,name,components,parent,children=()):
    s=header(1,ident,'GameObject')+'  serializedVersion: 6\n  m_Component:\n'
    s+=''.join(f'  - component: {{fileID: {c}}}\n' for c in [ident+1]+components)
    s+=f'  m_Layer: 0\n  m_Name: {name}\n  m_TagString: Untagged\n  m_IsActive: 1\n'
    s+=header(4,ident+1,'Transform')+f'  m_GameObject: {{fileID: {ident}}}\n  serializedVersion: 2\n  m_LocalRotation: {{x: 0, y: 0, z: 0, w: 1}}\n  m_LocalPosition: {{x: 0, y: 0, z: 0}}\n  m_LocalScale: {{x: 1, y: 1, z: 1}}\n'
    s+='  m_Children:'+ ('\n'+''.join(f'  - {{fileID: {c}}}\n' for c in children) if children else ' []\n')
    return s+f'  m_Father: {{fileID: {parent}}}\n'
allmeshes=meshes+[water]
chunks=obj(base,'Manhattan Surroundings',[base+2],0,[base+11+i*10 for i in range(len(allmeshes))])
chunks+=header(114,base+2,'MonoBehaviour')+f'  m_GameObject: {{fileID: {base}}}\n  m_Enabled: 1\n  m_Script: {{fileID: 11500000, guid: {guid(script.name)}, type: 3}}\n  m_EditorClassIdentifier: HunJam.Runtime::HunJam.ManhattanBackdrop\n  viewDistance: 6000\n'
for i,(name,meshguid,_) in enumerate(allmeshes):
    ident=base+10+i*10
    matguid=guid('ManhattanHarbor.mat' if name=='HarborWater' else 'ManhattanLandscape.mat')
    chunks+=obj(ident,name,[ident+2,ident+3],base+1)
    chunks+=header(33,ident+2,'MeshFilter')+f'  m_GameObject: {{fileID: {ident}}}\n  m_Mesh: {{fileID: 4300000, guid: {meshguid}, type: 2}}\n'
    chunks+=header(23,ident+3,'MeshRenderer')+f'  m_GameObject: {{fileID: {ident}}}\n  m_Enabled: 1\n  m_CastShadows: 0\n  m_ReceiveShadows: 0\n  m_DynamicOccludee: 0\n  m_LightProbeUsage: 0\n  m_ReflectionProbeUsage: 0\n  m_RenderingLayerMask: 1\n  m_Materials:\n  - {{fileID: 2100000, guid: {matguid}, type: 2}}\n  m_SortingOrder: 0\n'
# The old water is preserved as inactive instances, not deleted from the scene.
def disable_water(match):
    block=match[0]
    if not re.search(r'propertyPath: m_Name\n      value: WaterBlock_50m',block): return block
    target=re.search(r'- target: (\{fileID: \d+, guid: [a-f0-9]+, type: 3\})\n      propertyPath: m_Name',block)[1]
    entry=f'    - target: {target}\n      propertyPath: m_IsActive\n      value: 0\n      objectReference: {{fileID: 0}}\n'
    if entry not in block: block=block.replace('    m_Modifications:\n','    m_Modifications:\n'+entry,1)
    return block
scene=re.sub(r'--- !u!1001 &\d+\n.*?(?=--- !u!|\Z)',disable_water,scene,flags=re.S)
scene=scene.replace('--- !u!1660057539 &9223372036854775807',chunks+'--- !u!1660057539 &9223372036854775807')
scene=scene.replace('  m_Roots:\n',f'  m_Roots:\n  - {{fileID: {base+1}}}\n')
write(SCENE,scene)
print(f'Generated {len(allmeshes)} static meshes, {sum(m[2] for m in allmeshes)} triangles, {buildings} distant buildings.')
