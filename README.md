## CURRENTLY ONLY FOR v64 BETA.  
  
In vanilla, meteor showers have a fixed chance of happening each day at a rate of 0.7% chance.  
This mod allows you to customize that chance, and make it either a fixed rate, or make it linear/exponential which increases daily until one occurs, at which point it resets.  
  
You can also set a cap on the rate, so it never exceeds a certain point.  

## FORMULAS  
### Fixed Rate  
`chance = rate`  
  
### Linear Rate
`chance = days_since_last * rate`  
  
### Exponential Rate
`chance = rate ^ (-days_since_last) - 1`