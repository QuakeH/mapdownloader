from pyproj import Proj, transform
import urllib
import re
import ssl


inProj = Proj(init='epsg:4326')
outProj = Proj(init='epsg:5070')


lon1,lat1 = -88.604050,41.238060
lon2,lat2 = -88.188629,40.881333

lon1,lat1 = -117.489852905273,47.328119405234
lon2,lat2 = -116.983795166016,46.8831923266462

lon1,lat1 = -120.299606323242,36.5245359150048
lon2,lat2 = -119.09797668457,35.922420347285


lon1,lat1 = -91.1844635009766,34.9619345057769
lon2,lat2 = -90.6063079833984,34.4363631809337


lon1,lat1 = -102.82666, 34.32237324440533 
lon2,lat2 = -101.67325, 33.306789832353964  

#lon1,lat1 = -90.752075, 37.02528430302893 
#lon2,lat2 = -89.563416, 36.007451011934776  

#bbox: n,s,e,w

nlon1,nlat1 = transform(inProj,outProj,lon1,lat1)
nlon2,nlat2 = transform(inProj,outProj,lon2,lat2)


#https://nassgeodata.gmu.edu/CropScape/GetImage?dataurl=https://nassgeodata.gmu.edu/nass_data_cache/CDL_2018_clip_20190221193158_2110788401.zip&latlonbbox=-87.843488,41.078876,-87.717759,41.14771&projection=EPSG:102004
#https://nassgeodata.gmu.edu/axis2/services/CDLService/GetCDLFile?year=2018&points=175207,2219600,175207,2235525,213693,2235525,213693,2219600 
print("%d,%d" % (nlon1,nlat1))
print("%d,%d" % (nlon1,nlat2))
print("%d,%d" % (nlon2,nlat2))
print("%d,%d" % (nlon2,nlat1))
print("%d,%d" % (nlon1,nlat1))

bbstring = "%d,%d,%d,%d" %(nlon1,nlat2,nlon2,nlat1)

print(bbstring)

year = 2018

points = [37.02528430302893,-90.752075,36.995510653879045,-89.51941,36.007451011934776,-89.563416,36.03617591535172,-90.78052 ]
tiffname = "S2B_MSIL1C_20180302T164309_N0206_R126_T15SYA_20180302T215740"

bbstring = ""
for i in range(4):
    nlon,nlat = transform(inProj,outProj,points[i*2+1],points[i*2])
    print(nlon,nlat)
    bbstring += "%f,%f," %(nlon,nlat)
bbstring = bbstring[:-1]
print(bbstring)

# This restores the same behavior as before.

def get_cdl_image():
    for year in range(2012,2019):
        #cdlurl = "https://nassgeodata.gmu.edu/axis2/services/CDLService/GetCDLFile?year=%d&bbox=%s" % (year,bbstring)
        cdlurl = "https://nassgeodata.gmu.edu/axis2/services/CDLService/GetCDLFile?year=%d&points=%s" % (year,bbstring)
        print(cdlurl)
        context = ssl._create_unverified_context()
        content = urllib.urlopen(cdlurl, context=context).read()
        imgurl = re.sub(".*<returnURL>","",content)
        imgurl = re.sub("</returnURL>.*","",imgurl)
        urllib.urlretrieve(imgurl, "F:\data\cdl_%d_%s.tif" % (year,tiffname), context=context)

def get_cdl_stat():
    for year in range(2012,2019):
        #cdlurl = "https://nassgeodata.gmu.edu/axis2/services/CDLService/GetCDLStat?year=%d&bbox=%s&format=json" % (year,bbstring)
        cdlurl = "https://nassgeodata.gmu.edu/axis2/services/CDLService/GetCDLStat?year=%d&points=%s&format=json" % (year,bbstring)
        print(cdlurl)
        context = ssl._create_unverified_context()
        content = urllib.urlopen(cdlurl, context=context).read()
        imgurl = re.sub(".*<returnURL>","",content)
        imgurl = re.sub("</returnURL>.*","",imgurl)
        urllib.urlretrieve(imgurl, "F:\data\cdl_stat_%d_%s.json" % (year,tiffname), context=context)

get_cdl_image()
get_cdl_stat()




