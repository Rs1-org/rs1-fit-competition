export class CreateCityDto{
    name!:string;
    population!:number;
    isCapital:boolean=false;
    countryId:number=1;
    latitude:number=42;
    longitude:number=42;
    description:string='';
}